using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TAG.utility;

public class HealingTower : Tower
{
    public float delay = 1f;
    float cooldown = 1f;
    List<GameObject> targets;

    public override void Awake() {
        cooldown = Time.time+1f;
        base.Awake();
        targets = new List<GameObject>();
    }

    protected override void FixedUpdate()
    {
        #if UNITY_EDITOR
        if( Input.GetKeyDown(KeyCode.U) && this.tag == "Attacker") {
            //Debug.Log("Upgrade?");
            Upgrade(upgradeScriptableObject);
        }
        #endif

        
        if (cooldown < Time.time & GameController.currentPhase == G_STATE.EXECUTION_PHASE) {
            TargetAllyTowers();
            
            foreach(GameObject target in targets) {
                this.gun.Shoot(target.transform.position);
                //Debug.Log("pos: " + target.transform.position.ToString());
            }
            
            cooldown = Time.time + delay;
        }
        

    }

    protected virtual void TargetAllyTowers(){
        Collider2D[] results;

        targets.Clear();

        results = Physics2D.OverlapCircleAll(this.transform.position, this.range, ~0, -0.01f, 0.01f);
        GameObject[] objects = results.Select( hit => hit.gameObject ).ToArray();
        //results.ToList().ForEach( i => Debug.Log(i.collider.gameObject.name) ); 
        
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

                    //Debug.Log("object");
                    if((obj.CompareTag("Tower") && obj != this.gameObject)) {
                        targets.Add(obj);
                    }
                }
            }
    }
}
