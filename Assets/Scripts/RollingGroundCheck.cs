using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Diagnostics;

public class RollingGroundCheck : MonoBehaviour
{

    [SerializeField] private CharacterController2D controller;

    public Transform PlayerTransform;
    public float PlayerRayLength = 0.8f;
    public float rotationSmoothing = 0.1f;
    public bool rayhit = true;
    private float smoothVelocity = 0;
    private float Rotation;

    // Update is called once per frame
    void Update()
    {
        if (PauseMenu.GameIsPause)
        {
            return;
        }
    }

    void FixedUpdate()
    {
        //Raycast detection
        float angle = transform.eulerAngles.z;
        Vector2 rayDirection = new(Mathf.Cos((angle - 90) * Mathf.Deg2Rad), Mathf.Sin((angle - 90) * Mathf.Deg2Rad));

        RaycastHit2D PlayerRay = Physics2D.Raycast(PlayerTransform.position, rayDirection, PlayerRayLength, 1 << 0);

        if (PlayerRay.collider)
        {
            Debug.DrawRay(PlayerTransform.position, rayDirection * PlayerRayLength, Color.red, 0, true);
        }
        else
        {
            Debug.DrawRay(PlayerTransform.position, rayDirection * PlayerRayLength, Color.blue, 0, true);
        }

        float normalAngle = Mathf.Atan2(PlayerRay.normal.y, PlayerRay.normal.x) * Mathf.Rad2Deg;

        if (PlayerRay.collider)
        {
            float targetAngle = normalAngle - 90;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle, ref smoothVelocity, rotationSmoothing);

            transform.rotation = Quaternion.Euler(0, 0, smoothAngle);
        }
        else if (!PlayerRay.collider)
        {
            float turnAngle = Mathf.Atan2(PlayerRay.normal.y, PlayerRay.normal.x) * Mathf.Rad2Deg;
            float rotate = (controller.m_FacingRight ? +1 : -1) * 90;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, turnAngle, ref smoothVelocity, rotationSmoothing);

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