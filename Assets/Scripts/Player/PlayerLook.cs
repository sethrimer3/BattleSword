using UnityEngine;

namespace BattleSword.Player
{
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float sensitivity = 0.12f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private float pitch;

        public Vector2 LookInput { get; set; }

        private void Update()
        {
            var yaw = LookInput.x * sensitivity;
            pitch = Mathf.Clamp(pitch - LookInput.y * sensitivity, minPitch, maxPitch);

            transform.Rotate(Vector3.up * yaw);

            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }
    }
}
