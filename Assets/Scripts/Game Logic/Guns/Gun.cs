using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(FMODUnity.StudioBankLoader))]
[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public class Gun : MonoBehaviour{

    protected StudioEventEmitter sfxEmitter;

    protected GameObject bullet;
    protected float fireRate;
    protected int additionalDamage = 0;
    protected float additionalBulletSpeed = 0;
    protected int additionalPiercing = 0;
    protected float cooldown;

    public float initialFirerate = 1;
    public GameObject defaultBullet;
    public bool disabled = false;

    protected int bulletBaseDamage = 0; // At this point we're so far in development that I don't want to sort out a dynamic way to check a gun's bullet's damage

    // Start is called before the first frame update
    public virtual void Awake() {
        bullet = defaultBullet;

        fireRate = initialFirerate;
        this.cooldown = Time.time;

        bulletBaseDamage = GetBulletDamage();

        sfxEmitter = GetComponent<StudioEventEmitter>();
    }

    protected GameObject latestBullet;
    public void AddCooldown() {
        this.cooldown = Time.time + 0.1f;
        this.disabled = true;
    }
    
    public virtual void Shoot(Vector3 target, float barrelRadius=0f) {
        if(!this.disabled && target != null) {
            ShootingSound();

            this.cooldown = Time.time + 1f/fireRate;
            this.disabled = true;

            Vector2 diff = (Vector2)(target-transform.position);


            latestBullet  = Instantiate(bullet, transform.position, Quaternion.identity);
            latestBullet.transform.rotation = this.transform.parent.rotation;
            float dirRot = Vector2.SignedAngle(transform.right, diff);
            latestBullet.transform.Rotate(new Vector3(0, 0, dirRot));
            latestBullet.transform.position = latestBullet.transform.position + latestBullet.transform.right*barrelRadius;


            Bullet lb     = latestBullet.GetComponent<Bullet>();
            lb.Dmg       += additionalDamage;
            lb.Speed     += additionalBulletSpeed;
            lb.Piercing  += additionalPiercing;
            if(this.transform.parent.gameObject.layer == 7){
                latestBullet.layer = 6;
            }
            else {
                latestBullet.layer = 8;
            }
        }     
    }

    // Update is called once per frame
    private void FixedUpdate() {
        if(Time.time >= this.cooldown) {
            this.disabled = false;
        }
    }

    protected int GetBulletDamage(){
        Bullet b = this.Bullet.GetComponent<Bullet>();
        return b.damage;
    }

    public float Firerate{ get {return fireRate;}  set{fireRate = value;}}
    public int BulletBaseDamage{ get{return bulletBaseDamage;} set {bulletBaseDamage = value;}}
    public int AdditionalDamage{ get{return additionalDamage;} set {additionalDamage = value;}}
    public float AdditionalBulletSpeed{ get{return additionalBulletSpeed;} set {additionalBulletSpeed = value;}}
    public int AdditionalPiercing{ get{return additionalPiercing;} set {additionalPiercing = value;}}

    public GameObject Bullet {get {return bullet;} set {bullet = value;}}

    public virtual void ShootingSound() {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/shoot_or_hit_placeholder");
    }

    
}
