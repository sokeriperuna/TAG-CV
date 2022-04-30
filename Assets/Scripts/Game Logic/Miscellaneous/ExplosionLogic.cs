using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ExplosionLogic : MonoBehaviour
{
    public int explosionDamage = 50;

    private List<Entity> entities;
    private ExplosionFXController FXController;

    private void Awake() {
        entities = new List<Entity>();
        FXController = GetComponent<ExplosionFXController>();
    }

    private void ProccessExplosion(){
        if(FXController.FX_Radius/2 <= 0f)
            return;
        else{
            Collider2D[] results;
            results = Physics2D.OverlapCircleAll(this.transform.position, FXController.FX_Radius/2, ~0, -0.01f, 0.01f);
            GameObject[] objects = results.Select( hit => hit.gameObject ).ToArray();
            //results.ToList().ForEach( i => Debug.Log(i.collider.gameObject.name) ); 
            
            foreach (GameObject obj in objects ) {
                
                if(obj.CompareTag("Bullet")){
                    Destroy(obj);
                    continue;
                }

                Debug.DrawLine(transform.position, obj.transform.position, Color.green);

                if((obj != this.gameObject & (obj.CompareTag("Tower") )| obj.CompareTag("Attacker"))) {
                    Entity e = obj.GetComponent<Entity>();
                    foreach(Entity i in entities)
                        if(e == i)
                            return;

                    Debug.Log(this.gameObject.name + " => " + e.gameObject.name);
                    e.DoDamage(explosionDamage);
                    entities.Add(e);
                    continue;
                }
            }
            //Debug.Log(closestEnemy);
        }
    }

    private void FixedUpdate() {
        ProccessExplosion();
    }

}
