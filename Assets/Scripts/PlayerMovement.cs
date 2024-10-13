using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This code only works with player inputs
public class PlayerMovement : MonoBehaviour
{
    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;
    bool roll = false;
    bool puff = false;

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.GameIsPause)
        {
            return;
        }

        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Roll"))
        {
            roll = true;
        }
        else if (Input.GetButtonUp("Roll"))
        {
            roll = false;
        }

        if (Input.GetButton("Puff") && roll)
        {
            puff = true;
        }
        else if (Input.GetButtonUp("Puff") && roll)
        {
            puff = false;
        }
    }

    public void OnPuffing()
    {
        animator.SetBool("IsPuffing", true);
    }

    public void OnRolling(bool isRolling)
    {
        animator.SetBool("IsRolling", isRolling);
    }

    void FixedUpdate()
    {
        controller.Move(horizontalMove * Time.fixedDeltaTime, roll, puff);
    }
}
