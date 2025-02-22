using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttachableItem : MonoBehaviour
{
    public CharacterController2D controller;

    public float detachForce = 5f;

    private Transform item;
    private Collider2D itemCol;
    private Rigidbody2D itemRB;
    private GameObject target;
    private Vector3 posOffset;
    private bool is_attached = false;

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

        if (Input.GetKeyDown(KeyCode.V))
            if (is_attached)
                Detach();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            target = collision.gameObject;
            posOffset = item.position - target.transform.position;
            item.parent = target.transform;
            itemCol.enabled = false;
            is_attached = true;
            itemRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log(posOffset);
        }
    }

    private void Detach()
    {
        is_attached = false;
        itemRB.bodyType = RigidbodyType2D.Dynamic;
        itemRB.AddForce(posOffset * detachForce);
        item.parent = null;
        itemRB.constraints = RigidbodyConstraints2D.None;
        //yield return new WaitForSeconds(0.1f);
        itemCol.enabled = true;
    }
}