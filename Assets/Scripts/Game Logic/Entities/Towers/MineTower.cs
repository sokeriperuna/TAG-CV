using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MineTower : Tower
{
    protected Collider2D collider;

    protected SpriteRenderer spriteRenderer;
    protected bool detonated = false;
    protected Color defaultColor = Color.white;

    protected ExplosionFXController FX_Controller;

    public GameObject explosionPrefab;
    public float HPBarFadeout=0.5f;

    public override void Awake()
    {
        detonated = false;
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
        collider = GetComponent<Collider2D>();
    }

    private void Detonate(){
        // Make 100% sure we haven't detonated yet.
        if(detonated)
            return;
        else{
            detonated = true;
            this.collider.enabled = false;

            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            FX_Controller = explosion.GetComponent<ExplosionFXController>();
            FX_Controller.SetExplosionColor(defaultColor);
            FX_Controller.StartExplosion(this.range*2);

            HPBarController HPBar = GetComponentInChildren<HPBarController>();
            HPBar.gameObject.SetActive(false);

            //Fade effect is currently not in use
            //SpriteRenderer sr = HPBar.GetComponent<SpriteRenderer>();
            //StartCoroutine(FadeoutHPBar(sr));

            Debug.Log("Detonate?");
            this.Die();
        }
    }


    protected override void FixedUpdate()
    {
        if(!detonated)
            Target();

        if(detonated & FX_Controller != null){
            if(FX_Controller.ExplosionApexReached){
                playSoundOnDeath = false;
                base.Die();
            }
        }
    }
    public override void DoDamage(int damage)
    {
        if(!detonated)
            base.DoDamage(damage);
    }

    public override void Die()
    {
        if(!detonated)
        {
            Debug.Log("Dying and about to detonate.");
            Detonate();
        }
    }

    protected override GameObject Target()
    {
        Collider2D[] results;

        results = Physics2D.OverlapCircleAll(this.transform.position, this.range, ~0, -0.01f, 0.01f);
        GameObject[] objects = results.Select( hit => hit.gameObject ).ToArray();
        //results.ToList().ForEach( i => Debug.Log(i.collider.gameObject.name) ); 
        
        foreach (GameObject obj in objects ) {

            Debug.DrawLine(transform.position, obj.transform.position, Color.green);

            if((obj.tag != this.gameObject.tag) && (obj.tag == "Attacker")) {
                Debug.Log("TAG: " + obj.tag);
                deathResourceReward = 0;
                Detonate();
                return null;
            }
        }
        //Debug.Log(closestEnemy);
        return null;
    }

    // Currently not in use
    IEnumerator FadeoutHPBar(SpriteRenderer HPBarSR){
        float progress = 0;
        Color originalColor = HPBarSR.color;
        Color endColor = originalColor;
        endColor.a = 0; 

        do{
            progress += Time.deltaTime;

            Color newColor = originalColor;
            newColor.a = TAG.utility.MathUtility.EaseOutExp(1, 0, progress/HPBarFadeout);
            HPBarSR.color = newColor;
            yield return null;
        }while(progress<=1f);
    }
}
