using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock block;
    private Entity entity;

    private float progress;

    Vector3 offset = Vector3.zero;

    private void Awake() {
        // Get Refernces
        spriteRenderer = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
        entity = this.transform.parent.GetComponent<Entity>();

        // Initialization
        InitializePropertyBlock();
        UpdatePropertyBlock();
        spriteRenderer.SetPropertyBlock(block);
        spriteRenderer.color = this.transform.parent.GetComponent<SpriteRenderer>().color;

        offset = entity.transform.position - transform.position;
    }

    private void FixedUpdate() {
        // We update the property block each frame to diplay the correct properties
        UpdatePropertyBlock();
        spriteRenderer.SetPropertyBlock(block);
    }

    private void LateUpdate() {
        Transform p = this.transform.parent;
        //this.transform.parent = null;
        this.transform.position = entity.transform.position + Vector3.up;//+ offset;

        Quaternion newRot = new Quaternion();
        newRot.eulerAngles = Vector3.zero;
        this.transform.rotation = newRot;
        //this.transform.parent = p;
    }

    private void OnDisable() {
        LateUpdate();
    }

    public void UpdatePropertyBlock(){
        progress = entity.HP / entity.hitpoints; // We need a max HP variable in the future?
        block.SetFloat("_HPProg", progress);
    }

    private void InitializePropertyBlock(){
        /// NOTE:
        /// Overhaul shader and propertyblock system in the future for optimization
        /// Arithmetic calculations could be offset to the GPU to save CPU resources 
        Sprite mainSprite = spriteRenderer.sprite;
        block.SetTexture("_MainTex", mainSprite.texture);
        //block.SetColor ("_FillColor", transform.parent.GetComponent<SpriteRenderer>().color);
    }
}
