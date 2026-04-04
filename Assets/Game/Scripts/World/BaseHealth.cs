using System;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class BaseHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 20;

        public event Action<int, int> HealthChanged;
        public event Action Depleted;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public void ApplyDamage(int damage)
        {
            if (damage <= 0 || CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
            {
                Depleted?.Invoke();
            }
        }
    }
}

