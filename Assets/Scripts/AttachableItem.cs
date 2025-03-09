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
    private GameObject target; // ���۪�
    private Vector3 posOffset; // 1.ī�G�M���a���Z��(�T�w) 2.���a->ī�G ����V
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

        if (Input.GetKeyDown(KeyCode.V)) // "���U"V�ɡA�N���a���W���۪�ī�G����
            if (is_attached)
                Detach();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            itemRB.bodyType = RigidbodyType2D.Dynamic; // �Nī�G���A�ର"�ʺA"
            target = collision.gameObject;
            posOffset = item.position - target.transform.position;
            item.parent = target.transform; //�P���a�إߤl�����Y
            //itemCol.enabled = false; // ����ī�Gcollider
            is_attached = true;
            itemRB.constraints = RigidbodyConstraints2D.FreezeRotation; // �Nī�GZ�b��w
            Debug.Log(posOffset);
        }
    }

    private void Detach()
    {
        is_attached = false;
        itemRB.AddForce(posOffset * detachForce); // �Nī�G�ª��a�y�ЦVī�G����V�I�O
        item.parent = null; //�Ѱ��P���a���l�����Y
        itemRB.constraints = RigidbodyConstraints2D.None; // ����Z�b��w
        //yield return new WaitForSeconds(0.1f);
        //temCol.enabled = true; // �}��ī�Gcollider
    }
}