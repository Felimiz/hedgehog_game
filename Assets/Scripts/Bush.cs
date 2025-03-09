using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    public AttachableItem attachableItem;

    private void Awake()
    {
        Transform parent = transform.parent; //取得母物件
        if (parent != null)
        {
            attachableItem = parent.GetComponentInChildren<AttachableItem>(); //尋找其子物件中的AttachableItem
        }
    }

    private void OnTriggerStay2D(Collider2D collision) //偵測玩家是否在草叢內並碰到果實
    {
        if (collision.gameObject.CompareTag("Player") && attachableItem.is_attached)
        {
            Destroy(transform.gameObject);
        }
    }
}
