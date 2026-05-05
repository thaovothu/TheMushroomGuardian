using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowAttack : MonoBehaviour
{
    [SerializeField] float raycastDistance = 1000f; // khoảng cách raycast
    Transform arrowSpawnPoint;
    
    void Start()
    {
        // Find ArrowSpawnPoint tự động trên Player
        arrowSpawnPoint = transform.root.Find("ArrowSpawnPoint");
        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning("[BowAttack] ArrowSpawnPoint không tìm thấy! Sẽ dùng transform.position");
        }
        else
        {
            Debug.Log("[BowAttack] Found ArrowSpawnPoint at " + arrowSpawnPoint.position);
        }
    }
    
    // Được gọi từ animation event khi player bắn cung
    public void FireArrow()
    {
        Debug.Log("[BowAttack] FireArrow called");
        
        // Xác định vị trí bắn từ - dùng arrowSpawnPoint hoặc transform.position nếu không tìm thấy
        Vector3 firePosition = arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
        
        // Raycast từ camera qua mouse position để lấy hướng bắn
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 rayDirection = ray.direction.normalized;
        
        // Nếu raycast hit gì, bắn về phía object đó
        RaycastHit hit;
        Vector3 targetPoint = ray.origin + ray.direction * raycastDistance;
        
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            targetPoint = hit.point;
            Debug.Log($"[BowAttack] Raycast hit: {hit.collider.name}");
        }
        
        Vector3 arrowDirection = (targetPoint - firePosition).normalized;
        Quaternion fireRotation = Quaternion.LookRotation(arrowDirection);

        Debug.Log($"[BowAttack] Firing from position: {firePosition}, toward: {targetPoint}, direction: {arrowDirection}");

        // Lấy arrow từ pool với hướng chuột
        GameObject arrow = ArrowPool.GetArrow(firePosition, fireRotation);
        
        Debug.Log("[BowAttack] Bắn mũi tên!");
    }
}



