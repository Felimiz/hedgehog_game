using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PlayerMovement : MonoBehaviour
{

    private void Update()
    {
        if(Input.GetButton ("Horizontal"))
        {
            Debug.Log(Input.GetButton("Horizontal"));
        }
    }
}