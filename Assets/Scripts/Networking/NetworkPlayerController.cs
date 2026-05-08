using Unity.Netcode;
using UnityEngine;

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

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            // Only the owning client should render from this camera or receive audio.
            SetLocalComponents(IsOwner);

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

            if (Input.GetKeyDown(KeyCode.F))
            {
                RequestSpawnTestProjectileRpc(playerCamera.transform.position, playerCamera.transform.forward);
            }
        }

        private void HandleLook()
        {
            var mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            transform.Rotate(Vector3.up * mouseX);
            cameraPitch = Mathf.Clamp(cameraPitch - mouseY, -80f, 80f);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            var move = transform.TransformDirection(input) * moveSpeed;

            if (characterController.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (characterController.isGrounded && Input.GetKeyDown(KeyCode.Space))
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
