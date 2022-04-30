using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OmniDirectionalTower : Tower
{
    
    protected override GameObject Target()
    {

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

            }
        }
        //Debug.Log(closestEnemy);
        return closestEnemy;
    }


}
