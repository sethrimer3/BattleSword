using Unity.Netcode;
using UnityEngine;

namespace BattleSword.Networking
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NetworkProjectile : NetworkBehaviour
    {
        [SerializeField] private float lifetime = 4f;

        private Rigidbody body;
        private Vector3 initialVelocity;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
        }

        public void Initialize(Vector3 velocity)
        {
            initialVelocity = velocity;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                return;
            }

            // Server owns projectile physics, and NetworkTransform replicates it to clients.
            body.linearVelocity = initialVelocity;
            Invoke(nameof(Despawn), lifetime);
        }

        private void Despawn()
        {
            if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn(true);
            }
        }
    }
}
