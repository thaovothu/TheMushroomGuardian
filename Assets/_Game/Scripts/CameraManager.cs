using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : BaseSingleton<CameraManager>
{
    [Header("References")]
    [SerializeField] InputReader input;
    [SerializeField] CinemachineFreeLook freeLookVCam;

    [Header("Settings")]
    [SerializeField, Range(0.5f, 20f)] float speedMultiplier = 1f;

    [Header("Crosshair")]
    [SerializeField, Range(2f, 32f)] float crosshairSize = 10f;
    [SerializeField] Color crosshairColor = new Color(1f, 1f, 1f, 0.85f);

    bool cameraLookEnabled = true;
    bool cameraMovementLock;
    bool gameReady = false;     // false = auth/loading phase, ESC bị disabled
    Texture2D crosshairTex;

    void OnEnable()
    {
        input.Look += OnLook;
        GameEvent.Player.OnSpawned += OnPlayerSpawned;
    }

    void OnDisable()
    {
        input.Look -= OnLook;
        GameEvent.Player.OnSpawned -= OnPlayerSpawned;
    }

    void Start()
    {
        // Tắt hoàn toàn auto input của Cinemachine
        freeLookVCam.m_XAxis.m_InputAxisName = "";
        freeLookVCam.m_YAxis.m_InputAxisName = "";
        freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
        freeLookVCam.m_YAxis.m_InputAxisValue = 0f;

        crosshairTex = BuildCircleTexture(32);

        // Auth/loading phase: cursor tự do để click UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnPlayerSpawned(GameObject _)
    {
        gameReady = true;
        cameraLookEnabled = true;
        ApplyCameraLookState();
    }

    void OnDestroy()
    {
        if (crosshairTex != null) Destroy(crosshairTex);
    }

    void OnGUI()
    {
        if (!cameraLookEnabled || crosshairTex == null) return;

        var prev = GUI.color;
        GUI.color = crosshairColor;
        float x = (Screen.width - crosshairSize) * 0.5f;
        float y = (Screen.height - crosshairSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, crosshairSize, crosshairSize), crosshairTex);
        GUI.color = prev;
    }

    static Texture2D BuildCircleTexture(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r + 0.5f;
                float dy = y - r + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                // Anti-alias 1 pixel rìa.
                float a = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (!gameReady) return;

        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            cameraLookEnabled = !cameraLookEnabled;
            ApplyCameraLookState();
        }
    }

    void OnLook(Vector2 cameraMovement, bool isDeviceMouse)
    {
        if (cameraMovementLock) return;
        // Khi đã thoát chế độ xoay cam (ESC) thì bỏ qua input chuột để người chơi thao tác UI.
        if (isDeviceMouse && !cameraLookEnabled) return;

        float deviceMultiplier = isDeviceMouse ? 1f : Time.deltaTime;
        freeLookVCam.m_XAxis.m_InputAxisValue = cameraMovement.x * speedMultiplier * deviceMultiplier;
        freeLookVCam.m_YAxis.m_InputAxisValue = cameraMovement.y * speedMultiplier * deviceMultiplier;
    }

    void ApplyCameraLookState()
    {
        if (cameraLookEnabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Ăn delta chuột thừa của frame chuyển trạng thái để cam không bị "giật".
            StartCoroutine(DisableMouseForFrame());
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            freeLookVCam.m_XAxis.m_InputAxisValue = 0f;
            freeLookVCam.m_YAxis.m_InputAxisValue = 0f;
        }
    }

    IEnumerator DisableMouseForFrame()
    {
        cameraMovementLock = true;
        yield return new WaitForEndOfFrame();
        cameraMovementLock = false;
    }

    public void SetTarget(Transform target)
    {
        freeLookVCam.Follow = target;
        freeLookVCam.LookAt = target;

        freeLookVCam.OnTargetObjectWarped(
            target,
            target.position - freeLookVCam.transform.position - Vector3.forward
        );
    }
}
