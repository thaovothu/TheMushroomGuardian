using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Features.Player
{
    
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }
        public bool JumpPressed { get; private set; }

        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
        }

        public void OnJump(InputValue value)
        {
            JumpPressed = value.isPressed;
        }

        public void ConsumeJump()
        {
            JumpPressed = false;
        }

}}
