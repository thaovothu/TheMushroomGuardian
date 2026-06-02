using UnityEngine;

public class UIBillBoarding : MonoBehaviour
{
    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            // Cam có thể bị destroy khi đổi scene → tìm lại.
            cam = Camera.main;
            if (cam == null) return;
        }

        // Đồng bộ rotation với cam → text luôn đối mặt cam dù cam xoay bất kỳ góc nào.
        transform.rotation = cam.transform.rotation;
    }
}
