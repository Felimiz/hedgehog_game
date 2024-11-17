using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float Speed = 10.0f;
    public void Move(float move)
    {
        transform.Translate(move * Speed * Time.deltaTime, 0, 0);
    }
}