using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillBoarding : MonoBehaviour
{
    private Camera cam;
    void Awake() {
        cam = Camera.main;
    }
    void Update()
    {
        transform.forward = cam.transform.forward;
    }
}
