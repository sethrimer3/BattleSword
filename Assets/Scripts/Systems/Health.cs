using System;
using UnityEngine;

namespace BattleSword.Systems
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;

        private float currentHealth;

        public event Action<Health> Died;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0f;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            if (currentHealth <= 0f)
            {
                Died?.Invoke(this);
            }
        }
    }
}
