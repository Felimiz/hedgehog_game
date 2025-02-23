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
    bool puff = false;
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

        //Raycast detection
        float CurrentAngle = transform.eulerAngles.z;
        float facing = (controller.m_FacingRight ? +1 : -1); // 決定玩家移動方向
        Vector2 rayDirection = new(Mathf.Cos((CurrentAngle - 90) * Mathf.Deg2Rad), Mathf.Sin((CurrentAngle - 90) * Mathf.Deg2Rad)); // 決定偵測地面的射線的方向，相對於玩家X軸正向順時鐘轉九十度(法線)
        Vector2 turnrayDirection = new(Mathf.Cos((CurrentAngle - 180) * Mathf.Deg2Rad) * facing, Mathf.Sin((CurrentAngle - 180) * Mathf.Deg2Rad) * facing); // 玩家面對方向的反向(即X軸反向)，轉向時轉向

        RaycastHit2D headRay = Physics2D.Raycast(headTransform.position, rayDirection, mainrayLength, 1 << 0); // 位於玩家頭部的偵測射線，照玩家X軸法線發射
        RaycastHit2D bodyRay = Physics2D.Raycast(bodyTransform.position, rayDirection, mainrayLength, 1 << 0); // 身體中間的偵測射線，照玩家X軸法線發射
        RaycastHit2D tailRay = Physics2D.Raycast(tailTransform.position, rayDirection, mainrayLength, 1 << 0); // 尾部的偵測射線，照玩家X軸法線發射
        RaycastHit2D turnRayA = Physics2D.Raycast(turnTransformA.position, turnrayDirection, checkArayLength, 1 << 0); // 在玩家底部的射線，照玩家面對反向發射
        RaycastHit2D turnRayB = Physics2D.Raycast(turnTransformB.position, -turnrayDirection, checkBrayLength, 1 << 0); // 在刺蝟嘴邊的射線，照玩家面對方向發射

        if (!controller.m_wasRolling) // 移動模式時的射線偵測，畫出所有射線
        {
            if (headRay.collider)
            {
                Debug.DrawRay(headTransform.position, rayDirection * mainrayLength, Color.red, 0, true);
            }
            else
            {
                Debug.DrawRay(headTransform.position, rayDirection * mainrayLength, Color.blue, 0, true);
            }

            if (bodyRay.collider)
            {
                Debug.DrawRay(bodyTransform.position, rayDirection * mainrayLength, Color.red, 0, true);
            }
            else
            {
                Debug.DrawRay(bodyTransform.position, rayDirection * mainrayLength, Color.blue, 0, true);
            }

            if (tailRay.collider)
            {
                Debug.DrawRay(tailTransform.position, rayDirection * mainrayLength, Color.red, 0, true);
            }
            else
            {
                Debug.DrawRay(tailTransform.position, rayDirection * mainrayLength, Color.blue, 0, true);
            }

            if (turnRayA.collider)
            {
                Debug.DrawRay(turnTransformA.position, turnrayDirection * checkArayLength, Color.red, 0, true);
                rayhit = true;
            }
            else
            {
                Debug.DrawRay(turnTransformA.position, turnrayDirection * checkArayLength, Color.blue, 0, true);
                rayhit = false;
            }

            if (turnRayB.collider)
            {
                Debug.DrawRay(turnTransformB.position, -turnrayDirection * checkBrayLength, Color.red, 0, true);
            }
            else
            {
                Debug.DrawRay(turnTransformB.position, -turnrayDirection * checkBrayLength, Color.blue, 0, true);
            }


            if (!isRotating) // 是否執行Rotate()
            {
                if (!headRay.collider && !bodyRay.collider && tailRay.collider && turnRayA.collider && !turnRayB.collider) // 尾部和底部的射線偵測到地面
                {
                    turnAngle = Mathf.Atan2(turnRayA.normal.y, turnRayA.normal.x) * Mathf.Rad2Deg; // 取得玩家(底部射線)與地面的夾角
                    targetAngle = turnAngle - 90; // 將變量設定成正確旋轉角度
                    isRotating = true;
                }
                else if (turnRayB.collider) // 嘴邊射線偵測到地面
                {
                    turnAngle = Mathf.Atan2(turnRayB.normal.y, turnRayB.normal.x) * Mathf.Rad2Deg; // 取得玩家(嘴邊射線)與地面的夾角
                    targetAngle = turnAngle - 90;
                    isRotating = true;

                }
                else if (!headRay.collider && !bodyRay.collider && !tailRay.collider) // 頭部、身體、尾部射線皆未偵測到地面(不考慮底部&嘴邊)
                {
                    targetAngle = 0;
                    isRotating = true;
                }

                else // 當底部、嘴邊射線皆未偵測到地面，但頭部、身體、尾部個別偵測到地面時，將玩家對齊地面
                {
                    if (bodyRay.collider)
                    {
                        turnAngle = Mathf.Atan2(bodyRay.normal.y, bodyRay.normal.x) * Mathf.Rad2Deg;
                    }
                    else if (tailRay.collider)
                    {
                        turnAngle = Mathf.Atan2(tailRay.normal.y, tailRay.normal.x) * Mathf.Rad2Deg;
                    }
                    else if (headRay.collider)
                    {
                        turnAngle = Mathf.Atan2(headRay.normal.y, headRay.normal.x) * Mathf.Rad2Deg;
                    }
                    targetAngle = turnAngle - 90;
                    isRotating = true;
                }
            }

            if (isRotating)
            {
                Rotate();
            }
        }

        else // 滾動模式
        {
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