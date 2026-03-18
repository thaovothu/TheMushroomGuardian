using UnityEngine;

namespace Game.Features.Player.Domain
{
    [System.Serializable]
    public class PlayerMovementMotor
    {
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 5f;

        private CharacterController _controller;
        private Transform _ownerTransform;
        private Transform _cameraTransform;

        public void Initialize(CharacterController controller, Transform ownerTransform, Transform cameraTransform)
        {
            _controller = controller;
            _ownerTransform = ownerTransform;
            _cameraTransform = cameraTransform;
        }

        public void Tick(Vector2 input, float deltaTime)
        {
            if (_controller == null || _ownerTransform == null)
            {
                return;
            }

            Vector3 move = new Vector3(input.x, 0f, input.y);

            Vector3 forward = _cameraTransform != null ? _cameraTransform.forward : _ownerTransform.forward;
            Vector3 right = _cameraTransform != null ? _cameraTransform.right : _ownerTransform.right;
            forward.y = 0f;
            right.y = 0f;

            move = forward * move.z + right * move.x;
            _controller.Move(move * deltaTime * moveSpeed);

            if (move.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                _ownerTransform.rotation = Quaternion.Slerp(_ownerTransform.rotation, targetRotation, rotationSpeed * deltaTime);
            }
        }
    }
}
