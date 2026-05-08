using Unity.Netcode;
using UnityEngine;

namespace BattleSword.Networking
{
    public sealed class NetworkPlayerOwnerFilter : NetworkBehaviour
    {
        [SerializeField] private Camera[] ownerCameras = System.Array.Empty<Camera>();
        [SerializeField] private AudioListener[] ownerAudioListeners = System.Array.Empty<AudioListener>();
        [SerializeField] private Behaviour[] ownerOnlyBehaviours = System.Array.Empty<Behaviour>();
        [SerializeField] private Renderer[] hideForOwnerRenderers = System.Array.Empty<Renderer>();

        public override void OnNetworkSpawn()
        {
            EnsureReferences();
            SetOwnerOnlyState(IsOwner);
        }

        public override void OnNetworkDespawn()
        {
            SetOwnerOnlyState(false);
        }

        private void Awake()
        {
            EnsureReferences();
            SetOwnerOnlyState(false);
        }

        private void EnsureReferences()
        {
            if (ownerCameras == null || ownerCameras.Length == 0)
            {
                ownerCameras = GetComponentsInChildren<Camera>(true);
            }

            if (ownerAudioListeners == null || ownerAudioListeners.Length == 0)
            {
                ownerAudioListeners = GetComponentsInChildren<AudioListener>(true);
            }

            if (hideForOwnerRenderers == null || hideForOwnerRenderers.Length == 0)
            {
                hideForOwnerRenderers = FindOwnerHiddenRenderers();
            }
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

            foreach (var rendererToHide in hideForOwnerRenderers)
            {
                if (rendererToHide != null)
                {
                    rendererToHide.enabled = !active;
                }
            }

            if (active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private Renderer[] FindOwnerHiddenRenderers()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            var hiddenRenderers = new System.Collections.Generic.List<Renderer>();

            foreach (var rendererToCheck in renderers)
            {
                if (rendererToCheck == null)
                {
                    continue;
                }

                var current = rendererToCheck.transform;
                while (current != null && current != transform)
                {
                    if (current.name.StartsWith("SK_FP_CH_", System.StringComparison.Ordinal))
                    {
                        hiddenRenderers.Add(rendererToCheck);
                        break;
                    }

                    current = current.parent;
                }
            }

            return hiddenRenderers.ToArray();
        }

#if UNITY_EDITOR
        public void Configure(Camera[] cameras, AudioListener[] listeners, Behaviour[] behaviours, Renderer[] renderersHiddenForOwner)
        {
            ownerCameras = cameras;
            ownerAudioListeners = listeners;
            ownerOnlyBehaviours = behaviours;
            hideForOwnerRenderers = renderersHiddenForOwner;
        }
#endif
    }
}
