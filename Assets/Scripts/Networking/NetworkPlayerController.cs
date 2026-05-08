using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSword.Networking
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class NetworkPlayerController : NetworkBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioListener audioListener;
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private NetworkProjectile testProjectilePrefab;
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float projectileSpawnDistance = 1.4f;
        [SerializeField] private float projectileForwardSpeed = 12f;

        private readonly NetworkVariable<Color> bodyColor = new(Color.white);
        private CharacterController characterController;
        private Vector3 verticalVelocity;
        private float cameraPitch;
        private float bodyYaw;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            // Only the owning client should render from this camera or receive audio.
            SetLocalComponents(IsOwner);
            bodyYaw = transform.eulerAngles.y;

            bodyColor.OnValueChanged += OnBodyColorChanged;

            if (IsServer)
            {
                bodyColor.Value = Color.HSVToRGB((OwnerClientId * 0.17f) % 1f, 0.75f, 1f);
            }

            ApplyBodyColor(bodyColor.Value);
        }

        public override void OnNetworkDespawn()
        {
            bodyColor.OnValueChanged -= OnBodyColorChanged;
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            HandleLook();
            HandleMovement();

            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                RequestSpawnTestProjectileRpc(playerCamera.transform.position, playerCamera.transform.forward);
            }
        }

        private void HandleLook()
        {
            if (Mouse.current == null)
            {
                return;
            }

            var delta = Mouse.current.delta.ReadValue();
            var mouseX = delta.x * lookSensitivity * 0.1f;
            var mouseY = delta.y * lookSensitivity * 0.1f;

            bodyYaw += mouseX;
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -80f, 80f);
            transform.rotation = Quaternion.Euler(0f, bodyYaw, 0f);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            var moveX = 0f;
            var moveZ = 0f;

            if (keyboard.aKey.isPressed)
            {
                moveX -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                moveX += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                moveZ -= 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                moveZ += 1f;
            }

            var input = new Vector3(moveX, 0f, moveZ);
            input = Vector3.ClampMagnitude(input, 1f);

            var move = transform.TransformDirection(input) * moveSpeed;

            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (characterController.isGrounded && keyboard.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move((move + verticalVelocity) * Time.deltaTime);
        }

        [Rpc(SendTo.Server)]
        private void RequestSpawnTestProjectileRpc(Vector3 origin, Vector3 direction)
        {
            if (testProjectilePrefab == null)
            {
                return;
            }

            // Clients request the action, but only the server/host creates networked objects.
            var spawnPosition = origin + direction.normalized * projectileSpawnDistance;
            var projectile = Instantiate(testProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
            projectile.Initialize(direction.normalized * projectileForwardSpeed);
            projectile.NetworkObject.Spawn();
        }

        private void SetLocalComponents(bool active)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = active;
            }

            if (audioListener != null)
            {
                audioListener.enabled = active;
            }

            if (active)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnBodyColorChanged(Color _, Color nextColor)
        {
            ApplyBodyColor(nextColor);
        }

        private void ApplyBodyColor(Color color)
        {
            if (bodyRenderer != null)
            {
                bodyRenderer.material.color = color;
            }
        }
    }
}
