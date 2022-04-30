using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class FenceTower : Tower
{


    public int fenceDamage = 45;
    public GameObject fencePrefab;

    public List<Tower> connectedTowers;
    protected bool isDying =false;

    public override void Awake() {
        base.Awake();
        connectedTowers = new List<Tower>();
        isDying = false;
        Entity.entityDeath += OnEntityDeath;

    }

    protected void OnDestroy() {
        Entity.entityDeath -= OnEntityDeath;
    }

    public void AddConnectedTower(Tower newTower){

        // Check if other tower is a fence tower
        if(newTower is FenceTower)
            foreach(Tower t in (newTower as FenceTower).connectedTowers)
                if(this == t){
                    Debug.Log("Tried to connect already connected towers " + this.ToString() + " and " + newTower.ToString() + '.');
                    return;
                }

        foreach(Tower t in connectedTowers){
            if(t == newTower){
                Debug.Log("Tried to connect already connected towers " + this.ToString() + " and " + newTower.ToString() + '.');
                return;
            }
        }

        connectedTowers.Add(newTower);
        if(newTower is FenceTower)
            (newTower as FenceTower).connectedTowers.Add(this);

        SpawnFenceTo(newTower);
    }
    public override void  Die(){
        isDying=true;
        base.Die();
    }

    protected void OnEntityDeath(Entity e){
        if(e == this){
            return;
        }
        else{
            bool connectedTowerDied = false;
            foreach(Tower t in connectedTowers) {
                if(t == e)
                    connectedTowerDied = true;
            }

            if(connectedTowerDied){
                foreach(Tower t in connectedTowers){
                    if(t is FenceTower){
                        if((t as FenceTower).isDying)
                            continue;
                    }
                    t.Die();
                }

                if(!this.isDying)
                    this.Die();
            }
        }
    }

    protected void SpawnFenceTo(Tower other){
        ElectricFence fence = Instantiate(fencePrefab, this.transform.position, Quaternion.identity, this.transform).GetComponent<ElectricFence>();
        fence.SetPosition(this.transform.position, other.transform.position);
        fence.SetDamage(this.fenceDamage);
    }

    protected override void FixedUpdate()
    {
        // We make sure that we don't call any unnecesary targeting scripts
    }
}
