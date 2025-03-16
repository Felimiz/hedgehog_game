using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Progress;

public class CharacterController2D : MonoBehaviour
{
    [Range(0.7f, 5)][SerializeField] private float PuffRadius = 1.5f;                          // Amount of force added when the player jumps.
    [Range(0, 5)][SerializeField] private float PuffTime = 0.5f;
    [SerializeField] public float adhesionForceA = 30f;                         // Adhension force while on ground
    [SerializeField] public float adhesionForceB = 50f;                         // Adhension force while NOT on ground
    [SerializeField] private float rollingForce = 10f;
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;           // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Transform GroundAngle;                      // A position updating current ground angle
    [SerializeField] private RollingGroundCheck rollingGroundCheck;      //This one is for Checking the ground while rolling
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] private Collider2D m_RollDisableCollider;                  // A collider that will be disabled when rolling(Along with m_CrouchDisableCollider)
    [SerializeField] private CircleCollider2D RollingCollider;                // A collider that will be enabled when rolling

    const float k_GroundedRadius = .4f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    public bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    private float m_adhesionForce;
    private float RCOriginalRad; // the oringinal radius of rolling collider

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    public BoolEvent OnRollEvent;
    public bool m_wasRolling = false;
    public bool IsFalling = false;

    public PlayerMovement movement;
    
    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        RCOriginalRad = RollingCollider.radius;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

        if (OnRollEvent == null)
            OnRollEvent = new BoolEvent();

        if (movement == null)
            movement = GetComponent<PlayerMovement>();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        if (m_Rigidbody2D.velocity.y < 0 && !m_Grounded)
        {
            IsFalling = true;
        }
        else
        {
            IsFalling = false;
        }

        if (movement.rayhit && !m_Grounded)
        {
            m_adhesionForce = adhesionForceB;
        }
        else if (movement.rayhit && m_Grounded)
        {
            m_adhesionForce = adhesionForceA;
        }
        else
        {
            m_adhesionForce = 0f;
        }

        //Debug.Log(m_adhesionForce);

        
        if (m_Grounded)
        {
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("UnGrounded");
        }
        
    }
    public void Move(float move, bool crouch, bool puff, bool roll)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded && !roll)
        {

            // If crouching
            if (crouch && m_Grounded)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = move * 10f * transform.right;
            m_Rigidbody2D.AddForce(-transform.up * m_adhesionForce);
            // And then smoothing it out and applying it to the character
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should puff...
        if (puff)
        {
            Debug.Log("Puffing");
            RollingCollider.radius = Mathf.Lerp(RollingCollider.radius, PuffRadius, Mathf.Sqrt(PuffTime));
        }
        else
        {
            RollingCollider.radius = Mathf.Lerp(RollingCollider.radius, RCOriginalRad, Mathf.Sqrt(PuffTime));
        }
        // If the player roll...
        if (roll)
        {
            if (!m_wasRolling)
            {
                m_wasRolling = true;
                OnRollEvent.Invoke(true);
            }
            
            // Disable two capsule colliders, and enables the circle one
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = false;
            if (m_RollDisableCollider != null)
                m_RollDisableCollider.enabled = false;
            if (RollingCollider != null)
                RollingCollider.enabled = true;

            m_Rigidbody2D.constraints = RigidbodyConstraints2D.None; // UnFreeze the Rotation of the player
            if(Mathf.Abs(move) > 0)
            AddForceAtAngle(rollingForce, GroundAngle, (move > 0 ? +1 : -1)); // make player roll at the direction it faced and parellel to the ground

        }
        else
        {
            if (m_wasRolling)
            {
                m_wasRolling = false;
                OnRollEvent.Invoke(false);
            }

            // enable two capsule colliders, and disables the circle one
            if (m_CrouchDisableCollider != null)
                m_CrouchDisableCollider.enabled = true;
            if (m_RollDisableCollider != null)
                m_RollDisableCollider.enabled = true;
            if (RollingCollider != null)
                RollingCollider.enabled = false;

            m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void AddForceAtAngle(float force, Transform transform , int Direction)
    {
        float angle = transform.eulerAngles.z;
        Vector2 rayDirection = new(Mathf.Cos((angle) * Mathf.Deg2Rad), Mathf.Sin((angle) * Mathf.Deg2Rad));
        m_Rigidbody2D.AddForce(rayDirection * force * Direction * (rollingGroundCheck.Grounded? 1 :0)) ; 
    }
}