using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetingRadiusController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock block;
    private Entity entity;

    private float minRadius = 0f;
    private float maxRadius = 1f;
    private float fov = 360f;

    [Range(0,1f)]public float transparency = 0.02f;

    private Color radiusColor;
//dsd
    public void Awake() {
        // Get Refernces
        spriteRenderer = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
        entity = this.transform.parent.GetComponent<Entity>();

        fov = entity.actionFOV;

        // Set color
        Color entityColor = entity.GetComponent<SpriteRenderer>().color;
        spriteRenderer.color = new Color(entityColor.r, entityColor.g, entityColor.b, transparency);

        Transform p = transform.parent;
        transform.parent = null;
        transform.localScale = Vector3.one * 2 * entity.actionRange;
        transform.parent = p;

        // Initialization
        InitializePropertyBlock();
        UpdatePropertyBlock();
        spriteRenderer.SetPropertyBlock(block);
    }

    private void UpdatePropertyBlock(){
        block.SetFloat("_MinRadius", minRadius);
        block.SetFloat("_MaxRadius", maxRadius);
        block.SetFloat("_FieldOfView", fov);
    }

    private void InitializePropertyBlock(){
        /// NOTE:
        /// Overhaul shader and propertyblock system in the future for optimization
        /// Arithmetic calculations could be offset to the GPU to save CPU resources 
        Sprite mainSprite = spriteRenderer.sprite;
        block.SetTexture("_MainTex", mainSprite.texture);
    }
}
