using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
    [SerializeField] private bool m_AirControl = false;                         // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Collider2D m_RollDisableCollider;                  // A collider that will be disabled when rolling
    [SerializeField] private Collider2D m_RollAbleCollider;                     // A collider that will be enabled when rolling

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    [SerializeField] private float m_MaxRollSpeed = 10f;                        // Maximum speed during roll.
    [SerializeField] private float m_Acceleration = 1f;                         // Acceleration rate for rolling.
    [SerializeField] private float m_Deacceleration = 2f;                       // Deacceleration rate when not rolling.
    private float currentRollSpeed = 0f;                                        // Current speed during roll.

    [SerializeField] private float m_PuffExpandSpeed = 0.1f;                    // ���Ȫ��t��
    [SerializeField] private float m_PuffShrinkSpeed = 0.1f;                    // ���Y���t��
    [SerializeField] private float m_MaxPuffScale = 1.5f;                       // �̤j���Ȥ��
    private Vector3 originalScale;                                              // �쥻���Y��j�p
    private bool isPuffing;
    [SerializeField] private float PuffJumplimit = 7f;
    [SerializeField] private float m_PuffJumpForce = 50f;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnRollEvent;
    private bool m_wasRolling = false;
    private float rollDirection = 1f; // Current rolling direction (1 for right, -1 for left)

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnRollEvent == null)
            OnRollEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
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
    }

    public void Move(float move, bool roll, bool puff)
    {
        // Only control the player if grounded or airControl is turned on
        if (m_Grounded || m_AirControl)
        {
            // �O��½�u���A�A�p�G���a���� Shift ��
            if (roll)
            {
                if (!m_wasRolling)
                {
                    m_wasRolling = true;
                    OnRollEvent.Invoke(true);

                    // Disable the regular collider and enable the roll collider
                    if (m_RollDisableCollider != null)
                        m_RollDisableCollider.enabled = false;

                    if (m_RollAbleCollider != null)
                        m_RollAbleCollider.enabled = true;
                }

                // ��������J�ɤ~�[�t�u�ʡA�_�h�v����t
                if (Mathf.Abs(move) > 0.01f)
                {
                    // �P�_��J����V
                    float inputDirection = Mathf.Sign(move);

                    // �ˬd�O�_��V
                    if (inputDirection != rollDirection)
                    {
                        // �O�d�D�ʡA���ȧ��ܥ[�t����V
                        rollDirection = inputDirection;
                    }

                    // �O����e�t�סA���~��[�t����F��̤j�u�ʳt��
                    currentRollSpeed = Mathf.Min(currentRollSpeed + m_Acceleration * Time.fixedDeltaTime, m_MaxRollSpeed);
                }
                else
                {
                    // �S�����ʿ�J�ɡA�t�׳v����֪��쬰0
                    currentRollSpeed = Mathf.Max(currentRollSpeed - m_Deacceleration * Time.fixedDeltaTime, 0f);
                }

                // ���κu�ʳt�סA�ھڷ�e����V�i�沾��
                m_Rigidbody2D.velocity = new Vector2(rollDirection * currentRollSpeed, m_Rigidbody2D.velocity.y);
            }
            else
            {
                if (m_wasRolling)
                {
                    m_wasRolling = false;
                    OnRollEvent.Invoke(false);

                    // Re-enable the regular collider and disable the roll collider after the roll ends
                    if (m_RollDisableCollider != null)
                        m_RollDisableCollider.enabled = true;

                    if (m_RollAbleCollider != null)
                        m_RollAbleCollider.enabled = false;

                    // Reset the roll speed after the roll ends
                    currentRollSpeed = 0f;
                }

                // Normal movement
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
            }

            // �����޿�
            if (puff && roll)
            {
                isPuffing = true;

                // ���ȡG�v���W�[���⪺scale����F��̤j���Ȥ��
                Vector3 targetScale = originalScale * m_MaxPuffScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, m_PuffExpandSpeed * Time.fixedDeltaTime);
            }
            else if(!puff && roll)
            {
                isPuffing = false;

                // ���Y�G�v����_���⪺scale���l�j�p
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, m_PuffShrinkSpeed * Time.fixedDeltaTime);
            }
            else
            {
                isPuffing = false;
                // shirk to normol size while not rolling
                Vector3 inverseScale = new Vector3(1 / originalScale.x, 1 / originalScale.y, 1);
            }

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
            /*
            if(currentRollSpeed > PuffJumplimit) 
            {
                m_Rigidbody2D.AddForce(new Vector2(0f, m_PuffJumpForce));
            }
            */
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        float currentRollSpeedTemp = currentRollSpeed;

        // �O����e�t�סA�Ӥ��k�s�A��½��t�פ�V
        currentRollSpeed = currentRollSpeedTemp * -1; // �O�d����ȳt�סA���[�t�u���ܤ�V

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
