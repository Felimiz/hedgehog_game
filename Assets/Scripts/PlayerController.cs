using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .2f;   // How much to smooth out the movement 

    [Header("Colliders")]
    [SerializeField] private Collider2D m_RollDisableCollider;                  // A collider that will be disabled when rolling
    [SerializeField] private Collider2D m_RollAbleCollider;                     // A collider that will be enabled when rolling

    private Rigidbody2D m_Rigidbody2D;
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;

    [Header("Roll")]
    [SerializeField] private float m_MaxRollSpeed = 10f;                        // Maximum speed during roll.
    [SerializeField] private float m_Acceleration = 1f;                         // Acceleration rate for rolling.
    [SerializeField] private float m_Deacceleration = 2f;                       // Deacceleration rate when not rolling.
    private float RollSpeed = 0f;                                        // Current speed during roll.

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;
    [Serializable] public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnRollEvent;

    private bool Rolling = false;
    private float rollDirection = 1f; // Current rolling direction (1 for right, -1 for left)

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnRollEvent == null)
            OnRollEvent = new BoolEvent();
    }

    public void Move(float move, bool roll)
    {
            // �O��½�u���A�A�p�G���a���� Shift ��
            if (roll)
            {
                Rolling = true;
                OnRollEvent.Invoke(true);

                // �N���ʸI��������½�u�I����
                if (m_RollDisableCollider != null)
                    m_RollDisableCollider.enabled = false;

                if (m_RollAbleCollider != null)
                    m_RollAbleCollider.enabled = true;

                // ��������J�ɤ~�[�t�u�ʡA�_�h�v����t
                if (Mathf.Abs(move) > 0.01f)
                {
                    // �P�_��J��V
                    float inputDirection = Mathf.Sign(move);

                    // �ˬd�O�_��V
                    if (inputDirection != rollDirection)
                    {
                        // �O�d�D�ʡA���ȧ��ܥ[�t����V
                        rollDirection = inputDirection;
                    }

                    // �O����e�t�סA���~��[�t����F��̤j�u�ʳt��
                    RollSpeed = Mathf.Min(RollSpeed + m_Acceleration * Time.fixedDeltaTime, m_MaxRollSpeed);
                }
                else
                {
                    // �S�����ʿ�J�ɡA�t�׳v����֪��쬰0
                    RollSpeed = Mathf.Max(RollSpeed - m_Deacceleration * Time.fixedDeltaTime, 0f);
                }

                // ���κu�ʳt�סA�ھڷ�e����V�i�沾��
                m_Rigidbody2D.velocity = new Vector2(rollDirection * RollSpeed, m_Rigidbody2D.velocity.y);
            }
            else // Not rolling
            {
                Rolling = false;
                OnRollEvent.Invoke(false);

                // �N½�u�I�������������ʸI����
                 if (m_RollDisableCollider != null)
                     m_RollDisableCollider.enabled = true;

                if (m_RollAbleCollider != null)
                       m_RollAbleCollider.enabled = false;

                // Reset the roll speed after the roll ends
                RollSpeed = 0f;

                // Normal movement
                Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
                m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
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
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        float currentRollSpeedTemp = RollSpeed;

        // �O����e�t�סA�Ӥ��k�s�A��½��t�פ�V
        RollSpeed = currentRollSpeedTemp * -1; // �O�d����ȳt�סA���[�t�u���ܤ�V

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
