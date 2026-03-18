using UnityEngine;

namespace Game.Features.Player.Domain
{
    [System.Serializable]
    public class PlayerGravityMotor
    {
        [SerializeField] private float gravityValue = -9.81f;

        private CharacterController _controller;
        private Vector3 _playerVelocity;

        public void Initialize(CharacterController controller)
        {
            _controller = controller;
        }

        public void Tick(float deltaTime)
        {
            if (_controller == null)
            {
                return;
            }

            if (_controller.isGrounded && _playerVelocity.y < 0f)
            {
                _playerVelocity.y = 0f;
            }

            _playerVelocity.y += gravityValue * deltaTime;
            _controller.Move(_playerVelocity * deltaTime);
        }
    }
}
