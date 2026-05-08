using BattleSword.Systems;
using UnityEngine;

namespace BattleSword.Enemies
{
    [RequireComponent(typeof(Health))]
    public sealed class SimpleEnemyDeath : MonoBehaviour
    {
        [SerializeField] private float destroyDelay = 1.5f;

        private Health health;

        private void Awake()
        {
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Died -= OnDied;
        }

        private void OnDied(Health _)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
