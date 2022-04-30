using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
public class MusicController : MonoBehaviour
{
    private static FMOD.Studio.EventInstance music;
    // Start is called before the first frame update
    void Start()
    {
        music = FMODUnity.RuntimeManager.CreateInstance("event:/music/ActionPhaseMusic");
        music.start();
        music.release();
    }

    public void EnterPlanningPhase()
    {
        music.setParameterByNameWithLabel("Phase", "PlanningPhase");
    }

    public void EnterUpgradePhase() 
    {
        music.setParameterByNameWithLabel("Phase", "PlanningPhase");
    }

    public void EnterActionPhase()
    {
        music.setParameterByNameWithLabel("Phase", "ActionPhase");

    }
    public void StopMusic() 
    {
        music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
