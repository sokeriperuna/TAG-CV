using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;


public class Attacker : Entity
{
    public override void DeathSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/unit_death");
    }
    public override void PlayHurtSound() {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/placeholder_hit",Vector3.zero);
    }

    public GameObject shipLost;
    public override void Die()
    {
        Instantiate(shipLost,transform.position,Quaternion.identity);
        base.Die();
    }

    public void Move(Vector3 pos) {
        this.transform.position = pos;
    }
}


