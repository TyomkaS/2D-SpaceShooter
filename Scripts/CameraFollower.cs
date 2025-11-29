using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _targerTransform;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private float _smooth;

    // Update is called once per frame

    public void SetNewTarget(Transform newTarget)
    { _targerTransform = newTarget; }

    private void Move()
    {
        if (_targerTransform != null)
        {
            var nextPosition = Vector3.Lerp(transform.position, _targerTransform.position + _offset, Time.deltaTime * _smooth);
            transform.position = nextPosition;
        }
    }

    private void Update()
    {
        Move();
    }
}
