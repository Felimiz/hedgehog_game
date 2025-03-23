using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttachableItem : MonoBehaviour
{

    public float detachForce = 5f;

    private Transform item;
    private Collider2D itemCol;
    private Rigidbody2D itemRB;
    private GameObject target; // 附著者
    private Vector3 posOffset; // 1.蘋果和玩家的距離(固定) 2.玩家->蘋果 的方向
    public bool is_attached = false;

    private void Awake()
    {
        item = GetComponent<Transform>();
        itemCol = GetComponent<Collider2D>();
        itemRB = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        if (is_attached)
        {
            item.position = target.transform.position + posOffset;
        }

        if (Input.GetKeyDown(KeyCode.V)) // "按下"V時，將玩家身上附著的蘋果分離
            if (is_attached)
                Detach();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
        }
    }

    private void Detach()
    {
    }
}