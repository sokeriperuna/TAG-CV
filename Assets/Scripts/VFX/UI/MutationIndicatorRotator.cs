using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutationIndicatorRotator : MonoBehaviour
{
    public float rotationSpeed = 1f;

    private void Awake() {
        GetComponent<SpriteRenderer>().color = transform.parent.GetComponent<SpriteRenderer>().color;   
    }

    private void Update() {
        transform.Rotate(Vector3.back*rotationSpeed*Time.deltaTime);
    }
}
