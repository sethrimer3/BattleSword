using BattleSword.Systems;
using UnityEngine;

namespace BattleSword.Weapons
{
    public sealed class HitscanWeapon : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float range = 120f;
        [SerializeField] private LayerMask hitMask = ~0;

        public void Fire()
        {
            var source = aimCamera != null ? aimCamera.transform : transform;
            var ray = new Ray(source.position, source.forward);

            if (!Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            if (hit.collider.TryGetComponent<Health>(out var health))
            {
                health.TakeDamage(damage);
            }
        }
    }
}
