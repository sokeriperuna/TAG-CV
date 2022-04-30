using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FMODUnity;
using FMOD;
public class ElectricFence : MonoBehaviour
{    
    private FMOD.Studio.EventInstance fenceBuzz;

    protected int damage = 50;
    protected SpriteRenderer spriteRenderer;

    protected int counter = 0;

    public bool randomizeEffectPhaseAtStart = false;

    protected void Awake() {
        fenceBuzz = FMODUnity.RuntimeManager.CreateInstance("event:/action_phase/LoopingElectricBuzz3Timeline");
        fenceBuzz.start();
        fenceBuzz.release();

        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = this.transform.parent.GetComponent<SpriteRenderer>().color;
    }

    protected void Start() {
        if(randomizeEffectPhaseAtStart)
            RandomizeEffectPhase();
    }

    public void SetPosition(Vector2 start, Vector2 end){
        Vector2 averagePos = (start+end)/2f;
        transform.position = averagePos;

        Vector2 diff = end-start;

        float rotation = Vector2.SignedAngle(Vector2.right, diff); 
        
        transform.Rotate(new Vector3(0,0, rotation), Space.World);
        Vector2 positionSize = new Vector2((diff.magnitude-1), 1);
        spriteRenderer.size = positionSize;
        
        BoxCollider2D boxColl = GetComponent<BoxCollider2D>();
        boxColl.size = new Vector2(positionSize.x, 0.5f);
        boxColl.offset = new Vector2(0, -0.08f);

        //transform.localScale = new Vector3((diff.magnitude-1)/6.564103f, 1, 1); 
    } 

    public void SetDamage(int newDamage){
        damage = newDamage;
    }

    protected void OnTriggerEnter2D(Collider2D other) {
        switch(other.tag){
            case "Attacker":
            other.GetComponent<Attacker>().DoDamage(damage);
            break;

            case "Bullet":
            FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/Bullet_zap");
            Destroy(other.gameObject);
            break;

            default:
            break;
        }
    }
    public void StopFenceBuzz() 
    {
        fenceBuzz.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void OnDestroy() {
        StopFenceBuzz(); 
    }

    void RandomizeEffectPhase(){
        MaterialPropertyBlock matProp = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(matProp);
        float timeOffset = Random.Range(0f,1f);
        matProp.SetFloat("_TimeOffset", timeOffset);
    }
}
