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
            // 保持翻滾狀態，如果玩家按住 Shift 鍵
            if (roll)
            {
                Rolling = true;
                OnRollEvent.Invoke(true);

                // 將移動碰撞器換為翻滾碰撞器
                if (m_RollDisableCollider != null)
                    m_RollDisableCollider.enabled = false;

                if (m_RollAbleCollider != null)
                    m_RollAbleCollider.enabled = true;

                // 有水平輸入時才加速滾動，否則逐漸減速
                if (Mathf.Abs(move) > 0.01f)
                {
                    // 判斷輸入方向
                    float inputDirection = Mathf.Sign(move);

                    // 檢查是否轉向
                    if (inputDirection != rollDirection)
                    {
                        // 保留慣性，但僅改變加速的方向
                        rollDirection = inputDirection;
                    }

                    // 保持當前速度，並繼續加速直到達到最大滾動速度
                    RollSpeed = Mathf.Min(RollSpeed + m_Acceleration * Time.fixedDeltaTime, m_MaxRollSpeed);
                }
                else
                {
                    // 沒有移動輸入時，速度逐漸減少直到為0
                    RollSpeed = Mathf.Max(RollSpeed - m_Deacceleration * Time.fixedDeltaTime, 0f);
                }

                // 應用滾動速度，根據當前的方向進行移動
                m_Rigidbody2D.velocity = new Vector2(rollDirection * RollSpeed, m_Rigidbody2D.velocity.y);
            }
            else // Not rolling
            {
                Rolling = false;
                OnRollEvent.Invoke(false);

                // 將翻滾碰撞器切換為移動碰撞器
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

        // 保持當前速度，而不歸零，僅翻轉速度方向
        RollSpeed = currentRollSpeedTemp * -1; // 保留絕對值速度，不加速只改變方向

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
