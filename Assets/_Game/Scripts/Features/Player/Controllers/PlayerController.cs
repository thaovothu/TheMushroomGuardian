using UnityEngine;

namespace Game.Features.Player.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private Domain.PlayerMovementMotor movementMotor = new Domain.PlayerMovementMotor();
        [SerializeField] private Domain.PlayerGravityMotor gravityMotor = new Domain.PlayerGravityMotor();

        private CharacterController _controller;
        private Transform _cameraTransform;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _cameraTransform = Camera.main != null ? Camera.main.transform : null;

            if (inputHandler == null)
            {
                inputHandler = GetComponent<PlayerInputHandler>();
            }

            movementMotor.Initialize(_controller, transform, _cameraTransform);
            gravityMotor.Initialize(_controller);
        }

        private void Update()
        {
            Vector2 moveInput = inputHandler != null ? inputHandler.MoveInput : Vector2.zero;
            movementMotor.Tick(moveInput, Time.deltaTime);
            gravityMotor.Tick(Time.deltaTime);
        }
    }
}
