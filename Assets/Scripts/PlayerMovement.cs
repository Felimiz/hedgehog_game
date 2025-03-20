using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D controller;
    public Animator animator;

    public Transform headTransform;
    public Transform bodyTransform;
    public Transform tailTransform;
    public Transform turnTransformA;
    public Transform turnTransformB;

    public float mainrayLength = 0.3f;
    public float checkArayLength = 0.8f;
    public float checkBrayLength = 0.1f;
    public float runSpeed = 40f;
    public float rotationSmoothing = 0.1f;
    public bool rayhit = true;
    public bool isRotating = false;
    private float targetAngle;
    private float turnAngle;
    private float smoothVelocity = 0;

    float horizontalMove = 0f;
    public bool puff = false;
    bool crouch = false;
    bool roll = false;

    void Start()
    {
        headTransform = transform.Find("Head");
        if (headTransform == null)
        {
            Debug.LogError("Head object not found.");
        }
        bodyTransform = transform.Find("Body");
        if (bodyTransform == null)
        {
            Debug.LogError("Body object not found.");
        }
        tailTransform = transform.Find("Tail");
        if (tailTransform == null)
        {
            Debug.LogError("Tail object not found.");
        }
        turnTransformA = transform.Find("TurnCheckA");
        if (tailTransform == null)
        {
            Debug.LogError("TurnCheckA object not found.");
        }
        turnTransformB = transform.Find("TurnCheckB");
        if (tailTransform == null)
        {
            Debug.LogError("TurnCheckB object not found.");
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (PauseMenu.GameIsPause)
        {
            return;
        }

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButton("puff"))
        {
            puff = true;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsCrouching", false);
        }
        if (Input.GetButtonUp("puff"))
        {
            puff = false;
            animator.SetBool("IsJumping", false);
        }

        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        else if (Input.GetButtonUp("Crouch"))
        {
            crouch = false;
        }

        if (Input.GetButtonDown("Roll"))
        {
            roll = true;
        }
        else if (Input.GetButtonUp("Roll"))
        {
            roll = false;
        }
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
    }

    public void OnRolling(bool isRolling)
    {
        animator.SetBool("IsRolling", isRolling);
    }

    void FixedUpdate()
    {
        // Move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, puff, roll);

        // Raycast detection V2
        float CurrentAngle = transform.eulerAngles.z;
        RaycastHit2D headRay = Physics2D.Raycast(headTransform.position, -transform.up, mainrayLength, 1 << 0); // 位於玩家頭部的偵測射線，向玩家正下方發射
        RaycastHit2D tailRay = Physics2D.Raycast(tailTransform.position, -transform.up, mainrayLength, 1 << 0); // 尾部的偵測射線，向玩家正下方發射

        if (!controller.m_wasRolling) // 移動模式時的射線偵測，畫出所有射線
        {
            Debug.DrawRay(headTransform.position, -transform.up * mainrayLength, Color.red, 0, true);
            Debug.DrawRay(tailTransform.position, -transform.up * mainrayLength, Color.red, 0, true);

            if (headRay.collider && tailRay.collider)
            {
                rayhit = true;
                Vector3 headNormal = headRay.normal;
                Vector3 tailNormal = tailRay.normal;
                Vector3 aveNormal = (headNormal + tailNormal) / 2; //取偵測到的兩地面法線之平均值

                if (!isRotating)
                {
                    targetAngle = Mathf.Atan2(aveNormal.y, aveNormal.x) * Mathf.Rad2Deg - 90;
                    Rotate();
                }
            }
            /* 未偵測到地面時重置角度
            else
            {
                rayhit = false;

                if (!isRotating)
                {
                    targetAngle = 0;
                    Rotate();
                }
            }
            */

        }

    }

    void Rotate()
    {
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref smoothVelocity, rotationSmoothing); // 將當前玩家方向漸變到目標方向
        transform.rotation = Quaternion.Euler(0, 0, smoothAngle); // 反映變量
        if (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle)) < 0.1f) //如果玩家當前方向與欲轉動方向夾角<0.1度
        {
            transform.rotation = Quaternion.Euler(0, 0, targetAngle); // 直接轉動至該角度
            isRotating = false;
        }
    }
}