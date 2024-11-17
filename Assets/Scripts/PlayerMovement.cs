using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerMovement : MonoBehaviour
{
    public PlayerController controller;

    float HorizontalMove = 0f;

    private void Update()
    {
        HorizontalMove = Input.GetAxisRaw("Horizontal");
    }

    private void FixedUpdate()
    {
        controller.Move(HorizontalMove);
    }

}