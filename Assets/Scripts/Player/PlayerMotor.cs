using UnityEngine;

namespace BattleSword.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.2f;

        private CharacterController controller;
        private Vector3 verticalVelocity;

        public Vector2 MoveInput { get; set; }
        public bool WantsSprint { get; set; }
        public bool WantsJump { get; set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var speed = WantsSprint ? sprintSpeed : moveSpeed;
            var input = Vector3.ClampMagnitude(new Vector3(MoveInput.x, 0f, MoveInput.y), 1f);
            var worldMove = transform.TransformDirection(input) * speed;

            if (controller.isGrounded && verticalVelocity.y < 0f)
            {
                verticalVelocity.y = -2f;
            }

            if (controller.isGrounded && WantsJump)
            {
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity.y += gravity * Time.deltaTime;
            controller.Move((worldMove + verticalVelocity) * Time.deltaTime);
            WantsJump = false;
        }
    }
}
