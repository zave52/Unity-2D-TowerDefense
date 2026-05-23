using System;
using UnityEngine;

namespace TowerDefense.World
{
    public sealed class BaseHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 20;
        [SerializeField] private AudioClip damageSound;

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

            if (damageSound != null && TowerDefense.Core.AudioManager.Instance != null)
            {
                TowerDefense.Core.AudioManager.Instance.PlayPositionalSFX(damageSound, transform.position);
            }

            if (CurrentHealth == 0)
            {
                Depleted?.Invoke();
            }
        }
    }
}

