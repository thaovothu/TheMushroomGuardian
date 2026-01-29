using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Stats")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float gravityValue = -9.81f;

    private CharacterController _controller;
    private Vector2 _inputVector;
    private Vector3 _playerVelocity;
    private Transform _cameraTransform;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (Camera.main != null) _cameraTransform = Camera.main.transform;
    }
    public void OnMove(InputValue value)
    {
        Debug.Log("Joystick hoạt động! Giá trị: " + value.Get<Vector2>());
        _inputVector = value.Get<Vector2>();
    }   

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(_inputVector.x, 0, _inputVector.y);
        move = _cameraTransform.forward * move.z + _cameraTransform.right * move.x;
        move.y = 0; 
        _controller.Move(move * Time.deltaTime * moveSpeed);
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_controller.isGrounded && _playerVelocity.y < 0)
        {
            _playerVelocity.y = 0f;
        }

        _playerVelocity.y += gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }
}