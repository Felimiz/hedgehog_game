using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    public AttachableItem attachableItem;

    private void Awake()
    {
        Transform parent = transform.parent; //���o������
        if (parent != null)
        {
            attachableItem = parent.GetComponentInChildren<AttachableItem>(); //�M���l���󤤪�AttachableItem
        }
    }

    private void OnTriggerStay2D(Collider2D collision) //�������a�O�_�b���O���øI��G��
    {
        if (collision.gameObject.CompareTag("Player") && attachableItem.is_attached)
        {
            Destroy(transform.gameObject);
        }
    }
}
