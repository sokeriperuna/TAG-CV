using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayButtonClick : MonoBehaviour
{
    public static void PlayClickSound() {
        FMODUnity.RuntimeManager.PlayOneShot("event:/UI_sounds/Button click",Vector3.zero);
    }
}
