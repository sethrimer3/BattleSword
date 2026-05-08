using Unity.Netcode;
using UnityEngine;

namespace BattleSword.Networking
{
    public sealed class NetworkPlayerOwnerFilter : NetworkBehaviour
    {
        [SerializeField] private Camera[] ownerCameras = System.Array.Empty<Camera>();
        [SerializeField] private AudioListener[] ownerAudioListeners = System.Array.Empty<AudioListener>();
        [SerializeField] private Behaviour[] ownerOnlyBehaviours = System.Array.Empty<Behaviour>();

        public override void OnNetworkSpawn()
        {
            SetOwnerOnlyState(IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            SetOwnerOnlyState(false);
        }

        private void Awake()
        {
            SetOwnerOnlyState(false);
        }

        private void SetOwnerOnlyState(bool active)
        {
            foreach (var ownerCamera in ownerCameras)
            {
                if (ownerCamera != null)
                {
                    ownerCamera.enabled = active;
                }
            }

            foreach (var listener in ownerAudioListeners)
            {
                if (listener != null)
                {
                    listener.enabled = active;
                }
            }

            foreach (var behaviour in ownerOnlyBehaviours)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = active;
                }
            }

            if (active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

#if UNITY_EDITOR
        public void Configure(Camera[] cameras, AudioListener[] listeners, Behaviour[] behaviours)
        {
            ownerCameras = cameras;
            ownerAudioListeners = listeners;
            ownerOnlyBehaviours = behaviours;
        }
#endif
    }
}
