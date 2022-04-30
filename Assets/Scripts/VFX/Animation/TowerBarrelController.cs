using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBarrelController : MonoBehaviour
{
    Quaternion currentRot = Quaternion.identity;

    Tower tower;
    SpriteRenderer barrelSpriteRenderer;

    void Start() {
        float randomAngle = Random.Range(0, 360f);
        SetBarrelRotation(randomAngle);

        tower = transform.parent.GetComponent<Tower>();
        barrelSpriteRenderer = this.transform.GetChild(0).GetComponent<SpriteRenderer>();

        barrelSpriteRenderer.color = tower.GetComponent<SpriteRenderer>().color;

        
    }

    private void FixedUpdate() {

        Vector2 aim = tower.AimDir;
        if(aim != Vector2.zero)
            SetBarrelRotation(Vector2.SignedAngle(Vector2.up, aim));
    }

    void SetBarrelRotation(float newAngle){
        Vector3 eulerRot = new Vector3(0,0, newAngle);
        currentRot.eulerAngles = eulerRot;
        this.transform.rotation = currentRot;
    }
}
