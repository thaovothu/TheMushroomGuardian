using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageAnimation : MonoBehaviour
{
   public AnimationCurve opacityCurve;
   public AnimationCurve scaleCurve;
    public AnimationCurve heightCurve;
    private TextMeshProUGUI tmp;
    private float time = 0f;
    private Vector3 originPos;

    void Start()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        originPos = transform.position;
    }
    void Update()
    {
        tmp.color = new Color(1,1,1,opacityCurve.Evaluate(time));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(time);
        transform.position = originPos + new Vector3(0, 1+ heightCurve.Evaluate(time), 0);
        time += Time.deltaTime;
    }
}
