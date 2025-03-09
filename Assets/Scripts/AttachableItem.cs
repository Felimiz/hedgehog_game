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
            itemRB.bodyType = RigidbodyType2D.Dynamic; // 將蘋果狀態轉為"動態"
            target = collision.gameObject;
            posOffset = item.position - target.transform.position;
            item.parent = target.transform; //與玩家建立子母關係
            //itemCol.enabled = false; // 取消蘋果collider
            is_attached = true;
            itemRB.constraints = RigidbodyConstraints2D.FreezeRotation; // 將蘋果Z軸鎖定
            Debug.Log(posOffset);
        }
    }

    private void Detach()
    {
        is_attached = false;
        itemRB.AddForce(posOffset * detachForce); // 將蘋果朝玩家座標向蘋果的方向施力
        item.parent = null; //解除與玩家的子母關係
        itemRB.constraints = RigidbodyConstraints2D.None; // 取消Z軸鎖定
        //yield return new WaitForSeconds(0.1f);
        //temCol.enabled = true; // 開啟蘋果collider
    }
}