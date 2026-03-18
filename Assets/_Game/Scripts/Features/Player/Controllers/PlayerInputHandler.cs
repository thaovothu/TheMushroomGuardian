using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Features.Player.Controllers
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public Vector2 MoveInput { get; private set; }

        public void OnMove(InputValue value)
        {
            MoveInput = value.Get<Vector2>();
        }
    }
}
