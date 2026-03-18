using UnityEngine;
using UnityEngine.EventSystems;

public class LookDrag : MonoBehaviour, IDragHandler
{
    public MobileCameraLook camLook;

    public void OnDrag(PointerEventData eventData)
    {
        camLook.OnLook(eventData.delta);
    }
}