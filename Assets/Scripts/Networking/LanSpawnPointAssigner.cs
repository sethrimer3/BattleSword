using Unity.Netcode;
using UnityEngine;

namespace BattleSword.Networking
{
    public sealed class LanSpawnPointAssigner : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints = System.Array.Empty<Transform>();

        private NetworkManager networkManager;

        private void Awake()
        {
            networkManager = GetComponent<NetworkManager>();
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback += MoveClientToSpawnPoint;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback -= MoveClientToSpawnPoint;
            }
        }

        private void MoveClientToSpawnPoint(ulong clientId)
        {
            if (spawnPoints.Length == 0 || networkManager == null || !networkManager.IsServer)
            {
                return;
            }

            if (!networkManager.ConnectedClients.TryGetValue(clientId, out var client) || client.PlayerObject == null)
            {
                return;
            }

            var spawnPoint = spawnPoints[(int)(clientId % (ulong)spawnPoints.Length)];
            if (spawnPoint == null)
            {
                return;
            }

            var playerTransform = client.PlayerObject.transform;
            playerTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }
    }
}
