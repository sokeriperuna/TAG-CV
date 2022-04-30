using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTowerGun : Gun
{
    public override void ShootingSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/basic_tower_shoot");
    }
}
