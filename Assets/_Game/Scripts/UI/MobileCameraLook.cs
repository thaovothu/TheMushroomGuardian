using UnityEngine;
using Cinemachine;

public class MobileCameraLook : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;

    public float sensitivityX = 300f;
    public float sensitivityY = 2f;

    float touchX;
    float touchY;

    public void OnLook(Vector2 delta)
    {
        touchX = delta.x;
        touchY = delta.y;
    }

    void LateUpdate()
    {
        freeLookCam.m_XAxis.Value += touchX * sensitivityX * Time.deltaTime;
        freeLookCam.m_YAxis.Value += touchY * sensitivityY * Time.deltaTime;

        touchX = 0;
        touchY = 0;
    }
}