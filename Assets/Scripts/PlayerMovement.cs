using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D controller;
    public Animator animator;

    public Transform headTransform;
    public float rayLength = 0.8f;
    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool jump = false;
    bool crouch = false;
    bool roll = false;

    void Start()
    {
        headTransform = transform.Find("Head");
        if (headTransform == null)
        {
            Debug.LogError("Head object not found.");
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

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsCrouching", false);
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
        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump, roll);
        jump = false;

        //Raycast detection
        float angle = headTransform.eulerAngles.z;
        Vector2 rayDirection = new Vector2(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad));
        RaycastHit2D ray = Physics2D.Raycast(headTransform.position, rayDirection, rayLength, 1 << 0);
        if (ray.collider)
        {
            Debug.DrawRay(headTransform.position, rayDirection * rayLength, Color.red, 0, true);

            Vector2 groundNormal = ray.normal;
            float normalangle = Mathf.Atan2(groundNormal.y, groundNormal.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, normalangle - 90);
        }
        else
        {
            Debug.DrawRay(headTransform.position, rayDirection * rayLength, Color.blue, 0, true);
            if (controller.IsFalling)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }
}