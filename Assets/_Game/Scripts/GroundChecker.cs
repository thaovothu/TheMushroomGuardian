using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] float groundDistance = 0.5f;
    [SerializeField] float sphereRadius = 0f;
    [SerializeField] LayerMask groundLayer;

    public bool IsGrounded { get; private set; }
    
    void Update()
    {
        // Use SphereCast instead of Raycast - bắt slope tốt hơn
        IsGrounded = Physics.SphereCast(transform.position, sphereRadius, Vector3.down, out RaycastHit hit, groundDistance);
    
        // Draw debug line
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundDistance, 
                      IsGrounded ? Color.green : Color.red);
    }
}
