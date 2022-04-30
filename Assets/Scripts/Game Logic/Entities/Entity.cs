using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Entity : MonoBehaviour
{
    public delegate void EntityDelegate(Entity e);
    public static event EntityDelegate entityDeath;
    public int actionRange = 1;
    [Range(0f, 360f)] public float actionFOV = 360;
    public int hitpoints = 1;
    public int movementSpeed = 0;
    public GameObject initialGun;
    public float dstToBarrelEnd = 0.8f;

    protected int range;
    protected float fieldOfView;
    protected int hp;
    protected int speed;
    protected Gun gun;

    protected Color originalColor;

    protected bool playSoundOnDeath = true;

    private bool repair;

    private List<SpriteRenderer> spriteRenderers;
    private bool hurt = false;
    public GameObject currentTarget;

    public struct Upgrades {
        
        public int entityHp;
        public int entityRange;
        public float gunFireRate;
        public int bulletSpeed;
        public int bulletDamage;
        public int bulletPiercing;

        public GameObject gun;
        public GameObject bullet;

        public Upgrades(int hp, int r, float firerate, int bulletSpeed, int damage, int piercing, GameObject gun, GameObject bullet){
            this.entityHp = hp;
            this.entityRange = r;
            this.gunFireRate = firerate;
            this.bulletSpeed = bulletSpeed;
            this.bulletDamage = damage;
            this.bulletPiercing = piercing;
            this.gun = gun;
            this.bullet = bullet;
        }
    }
    
    Upgrades upgrades;

    public void Upgrade(EntityUpgrade pu) {


        // called when pressing upgrade tile and passed upgrade scriptable object as parameter

        upgrades = new Upgrades(pu.entityHp, pu.entityRange, pu.gunFireRate, pu.bulletSpeed, pu.bulletDamage, pu.bulletPiercing, pu.gun, pu.bullet);

        //upgrades entity stats
        this.hitpoints +=  upgrades.entityHp; 
        if(hitpoints<=0)
            hitpoints=1;
        //this.hp         = this.hitpoints; // removed healing during upgrades since healing between rounds is now done elsewhere
        this.range     += upgrades.entityRange;
        if(range <=0f){
            range=1;
        }
        

        //upgrades gun and bullet stats
        if(this.gun == null){
            this.gun = Instantiate(initialGun, transform.position, Quaternion.identity, this.transform).GetComponent<Gun>();
        }
        
        if(this.gun != null){

            if(upgrades.gun != null){
                
                this.gun = Instantiate(upgrades.gun, this.transform.position, Quaternion.identity).GetComponent<Gun>();
            }   

            if(upgrades.bullet != null) {
                this.gun.Bullet = upgrades.bullet;
            }
            
            this.gun.AdditionalDamage      += upgrades.bulletDamage;

            if(this.gun.AdditionalDamage + this.gun.BulletBaseDamage <= 0){
                this.gun.AdditionalDamage = -this.gun.BulletBaseDamage+1;
            }
            //if(this.gun.AdditionalDamage)

            this.gun.AdditionalBulletSpeed += upgrades.bulletSpeed;
            this.gun.AdditionalPiercing    += upgrades.bulletPiercing;
            this.gun.Firerate              *= 1f + upgrades.gunFireRate; // Upgrades multiply the existing firerate by a percetage
            this.gun.Firerate = Mathf.Round(this.gun.Firerate*10)/10; 

            if(this.gun.Firerate <= 0f){
                this.gun.Firerate = 0.1f;
            }

            //Debug.Log(this.gun.Firerate);

        }

    }

 

    [HideInInspector]public EntityUpgrade upgradeScriptableObject;
    
    public virtual void Awake() {
        spriteRenderers = new List<SpriteRenderer>();
        spriteRenderers.Add(this.GetComponent<SpriteRenderer>());
        foreach(SpriteRenderer sr in this.GetComponentsInChildren<SpriteRenderer>()){
            if(sr.CompareTag("Tower Barrel"))
                spriteRenderers.Add(sr);
        }
        originalColor = spriteRenderers[0].color;

        range     = actionRange;
        fieldOfView = actionFOV;
        hp        = hitpoints;
        speed     = movementSpeed;
        if(this.gun == null) {
            //Debug.Log("Gun not found on entity " + gameObject.name + ", instantiated default gun.");
            gun       = Instantiate(initialGun, transform.position, Quaternion.identity, this.transform).GetComponent<Gun>();
        }
        upgrades = new Upgrades(0, 0, 0, 0, 0, 0, null, null);
    }

    public virtual void Start() {
        this.currentTarget = null;
    }

    protected virtual void FixedUpdate() {

        #if UNITY_EDITOR
        if( Input.GetKeyDown(KeyCode.U) && this.tag == "Attacker") {
            //Debug.Log("Upgrade?");
            Upgrade(upgradeScriptableObject);
        }
        #endif

        currentTarget = this.Target();
        
        if(currentTarget != null && this.gun!= null && this.gun.Bullet != null) {
            this.gun.Shoot(currentTarget.transform.position, dstToBarrelEnd);
            
        }
    }

   
    public virtual void Die() {
        if(entityDeath != null)
            entityDeath(this); // Pass reference of dying self for game controller to handle.
        if(playSoundOnDeath)
            DeathSound();
        Destroy(this.gameObject);
    }
    public virtual void DeathSound(){
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/placeholder_death",Vector3.zero);

    }
    protected virtual GameObject Target() {
        Collider2D[] results;

        results = Physics2D.OverlapCircleAll(this.transform.position, this.range, ~0, -0.01f, 0.01f);
        GameObject[] objects = results.Select( hit => hit.gameObject ).ToArray();
        //results.ToList().ForEach( i => Debug.Log(i.collider.gameObject.name) ); 
        
        GameObject closestEnemy = null;
        if(objects.Length > 0)
        {
            float halfFOV = fieldOfView/2f;
            foreach (GameObject obj in objects ) {

             //Check if potential targets are within FOV
                if(fieldOfView<360f){
                    // Compare angles

                    Vector3 dirToTarget = obj.transform.position - transform.position;
                    float targetingAngle = Vector2.Angle(transform.right, dirToTarget);


                    if(targetingAngle > halfFOV) // Angle is too large and is not within FOV.
                        continue; // Go to next loop and evaluate the next target.
                }

                
                Debug.DrawLine(transform.position, obj.transform.position, Color.green);

                if(closestEnemy == null && (obj.tag != this.gameObject.tag) && (obj.CompareTag("Attacker") || obj.CompareTag("Tower"))) {
                    closestEnemy = obj;

                }


                //Debug.Log("object");
                if((obj.tag != this.gameObject.tag) && (obj.tag == "Attacker" || obj.tag == "Tower")) {

                    // Check if our potential target is within view

                    //Debug.Log("another entity found");
                    if((closestEnemy.transform.position - transform.position).sqrMagnitude > (obj.transform.position - transform.position).sqrMagnitude && !(closestEnemy.CompareTag("Untagged")))  {
                        closestEnemy = obj;
                        //Debug.Log("new closest enemy found");
                    }
                }
            }
        }
        //Debug.Log(closestEnemy);
        return closestEnemy;
    }
    public virtual void PlayHurtSound() {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/hit_enemy_placeholder",Vector3.zero);
    }

    public virtual void DoDamage(int damage) {
        // returns true if should die
        PlayHurtSound();
        if(!hurt)
            StartCoroutine(HurtEffect());
        //Debug.Log("Ouch! Oof! I took " + damage.ToString() + " damage!");
        

        this.hp -= damage;
        if(this.hp > hitpoints) {
            this.hp = hitpoints;
        }
        if(hp <= 0 ) {
            this.Die();
        }
    }

    public float GetGunFirerate(){
        float output =new float();
        output = this.gun.Firerate;
        return output;
    }

    public int GetGunDamage(){
        int output = new int();
        output = gun.BulletBaseDamage + gun.AdditionalDamage;
        return output;
    }

    public virtual Color HurtColor() {
        return Color.white;
    }
    
    IEnumerator HurtEffect() {
        hurt = true;
        var originalColour = spriteRenderers[0].color;
        foreach(SpriteRenderer sr in spriteRenderers)
            sr.color = HurtColor();
        
        yield return new WaitForSeconds(0.08f);

        foreach(SpriteRenderer sr in spriteRenderers)
            sr.color = originalColour;
        
        hurt = false;
    }

    public void Heal(int healing) { hp += healing; }
    public void HealToMax() { hp = hitpoints; }

    public float HP { get { return hp;}} // Read only accecessor
    public int Range{ get {return range;} protected set{range = value;}} //publicly read only protected write
}
