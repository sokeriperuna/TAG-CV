using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OmniDirectionalGun : Gun
{

    public int projectileCount = 8;

    public override void Shoot(Vector3 target, float barrelRadius=0f)
    {
        if(!this.disabled && target != null) {
            ShootingSound();


            this.cooldown = Time.time + 1f/fireRate;
            this.disabled = true;

            for(int i=0; i<projectileCount; i++){

                latestBullet  = Instantiate(bullet, transform.position, Quaternion.identity);
                latestBullet.transform.rotation = this.transform.parent.rotation;
                float dirRot = (360f/projectileCount)*i; // We spread the projectiles acros 360 degrees
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
    }
    public override void ShootingSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/omni_tower_shoot");
    }
}
