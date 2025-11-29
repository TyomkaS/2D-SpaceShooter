using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    interface IDamagebale
    {
        public void Die();
        public void TakeDamage(Vector3 damage);

        public void TakeScalarDamage(float damage);
    }
}
