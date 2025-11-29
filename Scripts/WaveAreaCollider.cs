using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WaveAreaCollider : MonoBehaviour
{
    [SerializeField] string targetAwaiterTag;

    public Action<GameObject, GameObject> OnEnter;
    public Action<GameObject, GameObject> OnExit;


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(targetAwaiterTag))
        {
            Debug.Log("ObjectName is " + other.gameObject.name);
            OnEnter?.Invoke(this.gameObject, other.gameObject);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(targetAwaiterTag))
        {
            Debug.Log("ObjectName is " + other.gameObject.name);
            OnExit?.Invoke(this.gameObject, other.gameObject);
        }  
    }
}
