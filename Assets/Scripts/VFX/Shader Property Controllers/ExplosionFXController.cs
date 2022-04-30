using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TAG.utility;

public class ExplosionFXController : MonoBehaviour
{
    private static int sortingIndex = 0;

    public float animSpeed = 1f;
    public float explosionRadius = 1f;
    public float stripeCount = 10f;

    private bool explosionApexReached = false;

    #if UNITY_EDITOR
    public bool debugExplodeOnStart = false;
    public float debugExplosionRadius = 1f;
    #endif

    private float currentRadius = 0f;

    private MaterialPropertyBlock propertyBlock;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider2D;

    private void Awake() {
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void Start() {
        spriteRenderer.sortingOrder = sortingIndex++; // We always put explosion one increment up in the VFX sorting order

        #if UNITY_EDITOR
        if(debugExplodeOnStart)
            StartExplosion(debugExplosionRadius);
        #endif
    }

    public void StartExplosion(float radius){
        StartCoroutine(ExplosionAnimation(animSpeed, radius, stripeCount));
    }

    IEnumerator ExplosionAnimation(float speed, float FX_radius, float _StripeCount){
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/Placeholder_Mine_Tower_Explosion_2",Vector3.zero);

        float progress = 0;
        do{
            float t = MathUtility.Sawtooth(progress, 1/animSpeed);


            currentRadius =  FX_radius * MathUtility.EaseOutExp(0, 1, t);
            Vector3 scale = Vector3.one * currentRadius;
            transform.localScale = scale;

            float stripes = MathUtility.EaseOutExp(1, _StripeCount, t);

            propertyBlock.SetFloat("_StripeCount", stripes);
            propertyBlock.SetFloat("_OuterCircleRadius", 1f);

            spriteRenderer.SetPropertyBlock(propertyBlock);

            //circleCollider2D.enabled = t<0.5f;

            progress += Time.deltaTime;

            yield return null;
        }while(progress <= 1f);

        explosionApexReached = true;

        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/Placeholder_Mine_Tower_Explosion",Vector3.zero);
        progress -= 1;
        do{

            float t = MathUtility.EaseOutExp(1, 0, progress);
            currentRadius = t*FX_radius;

            propertyBlock.SetFloat("_OuterCircleRadius", t);
            spriteRenderer.SetPropertyBlock(propertyBlock);


            progress += Time.deltaTime;
            yield return null;
        }while(progress <= 1f);

        Destroy(this.gameObject);
    }

    public void SetExplosionColor(Color c){ spriteRenderer.color = c; }

    public bool ExplosionApexReached {get { return explosionApexReached; }}

    public float FX_Radius {get { return currentRadius; }}

}
