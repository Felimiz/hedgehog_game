using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    public float floatSpeed = 1f;

    private void FixedUpdate()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector3(0, floatSpeed, 0);   //���t�ת��u�W�ɡA�������׼v�T
    }
}
