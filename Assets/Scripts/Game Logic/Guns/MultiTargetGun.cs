using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTargetGun :  Gun
{
    //This gun requires regulation from the tower so add cooldown directly to the tower/unit.
    public override void Shoot(Vector3 target, float barrelRadius=0f) {
        //Debug.Log("Shoot:" + target.ToString());
        ShootingSound();

        Vector2 diff = (Vector2)(target-transform.position);

        latestBullet  = Instantiate(bullet, transform.position, Quaternion.identity);
        latestBullet.transform.rotation = this.transform.parent.rotation;
        float dirRot = Vector2.SignedAngle(transform.right, (Vector2)(target-transform.position));
        latestBullet.transform.Rotate(new Vector3(0, 0, dirRot));
        latestBullet.transform.position = latestBullet.transform.position + latestBullet.transform.right*barrelRadius;


        Bullet lb     = latestBullet.GetComponent<Bullet>();
        lb.Dmg       += additionalDamage;
        lb.Speed     += additionalBulletSpeed;
        lb.Piercing  += additionalPiercing;
        if(this.transform.parent.gameObject.layer == 7){
            latestBullet.layer = 10;
        }
        else {
            latestBullet.layer = 10;
        }
           
    }
}
