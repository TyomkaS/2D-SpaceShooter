using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;
using System;

public class EnemyTypeC : EnemyTypeA
{

    [SerializeField] private Transform bullet;
    [SerializeField] private float reloadTime;
    
    protected override void Attack()
    {
        if (target != null)
        {
            float deltaX = transform.position.x - target.transform.position.x;
            float deltaZ = transform.position.z - target.transform.position.z;
            float distance = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaZ, 2));

            if (distance > targetOffset)
            {
                MoveTo(target.transform);
            }

            Debug.Log("Enemy C is Attacking");
            Shoot();

        }
        else
        {
            _isAttackmode = false;
        }

        
    }

    //protected override void Bump(Collision collision)
    //{
    //    Debug.Log("EnemyC Asteroid collision " + collision.impulse + " Velocity=" + _rb.velocity + " ObjectName" + collision.gameObject.name);
    //    TakeDamage(collision.impulse * collisionDamageMultiplayer * 1f);
    //    if (_knockoutRemainder <= 0)
    //    {
    //        _knockoutRemainder = knockoutTime;
    //    }
    //    //_rb.AddForce(new Vector3(collision.impulse.x * collisionImpulseMultiplayer, 0.0f, collision.impulse.z * collisionImpulseMultiplayer), ForceMode.Impulse);
    //}

    public override void Die()
    {
        if (!_isDeathActivated)
        {
            Transform ballObj = Instantiate(dyingball, transform.position, Quaternion.identity);
            DamageBall ball = ballObj.GetComponent<DamageBall>();

            ball.maximumScale = 30;

            ball.followTo = transform;
            _dieDelay = (ball.maximumScale / ball.growthSpeed) * 0.1f;
            _isDeathActivated = true;
        }
        else
        {
            if (_dieDelay > 0)
            {
                _dieDelay -= Time.deltaTime;
            }
            else
            {
                OnDestroy?.Invoke(this.gameObject, "Type C");
                Destroy(this.gameObject);
            }
        }
    }

    protected override void DebugHelathChange()
    {
        Debug.Log("Enemy Type C health=" + health.ToString());
    }

    private void Shoot()
    {
        if (_reloadTime <= 0)
        {
            Vector3 startposition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Transform localbullet = Instantiate(bullet, startposition, Quaternion.identity);
            localbullet.rotation = transform.rotation;
            DamageBall pb = localbullet.GetComponent<DamageBall>();
            pb.followTo = transform;
            _reloadTime = reloadTime;
        }
    }

    
}
