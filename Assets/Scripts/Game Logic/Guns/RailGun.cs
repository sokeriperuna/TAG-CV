using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGun : Gun
{
    public override void ShootingSound() {
        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/railgun_shoot");
    }
}
