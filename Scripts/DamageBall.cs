using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class DamageBall : MonoBehaviour
{
    public float maximumScale;
    public float growthSpeed;
    public string targetTag;
    public float damage;
    public Transform followTo;
    private Vector3 upDatedScale;

    private void OnEnable()
    {
        upDatedScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    private void MakeDamage(Collider other)
    {
        if (targetTag!=null && targetTag.Length!=0)
        {
            if (other.gameObject.CompareTag(targetTag))
            {
                IDamagebale targetFordamage = other.gameObject.GetComponent<IDamagebale>();

                if (targetFordamage != null)
                {
                    targetFordamage.TakeScalarDamage(damage);
                }
            }
        }   
    }

    void OnTriggerEnter(Collider other)
    {
        MakeDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        MakeDamage(other);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.localScale.x < maximumScale)
        {
            upDatedScale = upDatedScale * (1 + Time.deltaTime * growthSpeed);
            transform.localScale = upDatedScale;

            if (followTo!=null)
            {
                transform.position = followTo.position;
            }    
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
