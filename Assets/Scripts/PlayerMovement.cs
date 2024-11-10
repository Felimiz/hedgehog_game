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
    public Transform bodyTransform;
    public float headrayLength = 0.8f;
    public float bodyrayLength = 0.8f;
    public float runSpeed = 40f;
    public float rotationSmoothing = 0.1f;
    public bool rayhit = true;
    private float smoothVelocity = 0;

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
        bodyTransform = transform.Find("Body");
        if (bodyTransform == null)
        {
            Debug.LogError("Body object not found.");
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
        float angle = transform.eulerAngles.z;
        Vector2 rayDirection = new(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad));

        RaycastHit2D headRay = Physics2D.Raycast(headTransform.position, rayDirection, headrayLength, 1 << 0);
        RaycastHit2D bodyRay = Physics2D.Raycast(bodyTransform.position, rayDirection, bodyrayLength, 1 << 0);

        if (headRay.collider)
        {
            Debug.DrawRay(headTransform.position, rayDirection * headrayLength, Color.red, 0, true);
        }
        else
        {
            Debug.DrawRay(headTransform.position, rayDirection * headrayLength, Color.blue, 0, true);
        }

        if (bodyRay.collider)
        {
            Debug.DrawRay(bodyTransform.position, rayDirection * bodyrayLength, Color.red, 0, true);
        }
        else
        {
            Debug.DrawRay(bodyTransform.position, rayDirection * bodyrayLength, Color.blue, 0, true);
        }

        float normalAngle = Mathf.Atan2(headRay.normal.y, headRay.normal.x) * Mathf.Rad2Deg;

        if (headRay.collider)
        {
            float targetAngle = normalAngle - 90;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref smoothVelocity, rotationSmoothing);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
        else if (!headRay.collider && bodyRay.collider)
        {
            float rotate = (controller.m_FacingRight ? +1 : -1) * 45.0f;
            float targetAngle = angle - rotate;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref smoothVelocity, rotationSmoothing);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
        else
        {
            float targetAngle = 0;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref smoothVelocity, rotationSmoothing);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
            rayhit = false;
        }
    }
}