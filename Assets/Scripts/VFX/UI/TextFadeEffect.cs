using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFadeEffect : MonoBehaviour
{
    public Color primaryColor;
    public Color secondaryColor;

    public float flickerSpeed = 1f;

    RectTransform textTransform;
    TextMeshPro tmp;
    float endTime = 0;
    void Start() {
        tmp = GetComponent<TextMeshPro>();
        textTransform = gameObject.GetComponent<RectTransform>();
        endTime = Time.time+2.0f;
    }
    
    void Update()
    {   
        float t = Mathf.Abs(Mathf.Sin(Time.time*flickerSpeed));
        Color c = Color.Lerp(primaryColor, secondaryColor, t);
        c.a = Mathf.Pow((endTime-Time.time),2);
        tmp.color = c;
        textTransform.position += Vector3.up * (Time.deltaTime/2);
        if(Time.time >= endTime){
            Destroy(this.gameObject);
        }   
    }
}
