using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSword.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(PlayerLook))]
    public sealed class PlayerInputBridge : MonoBehaviour
    {
        private PlayerMotor motor;
        private PlayerLook look;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            look = GetComponent<PlayerLook>();
        }

        public void OnMove(InputValue value)
        {
            motor.MoveInput = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            look.LookInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            if (value.isPressed)
            {
                motor.WantsJump = true;
            }
        }

        public void OnSprint(InputValue value)
        {
            motor.WantsSprint = value.isPressed;
        }
    }
}
