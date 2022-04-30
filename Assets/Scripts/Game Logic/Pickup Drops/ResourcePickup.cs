using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class ResourcePickup : Pickup
{
    protected TMP_Text infoText;   
    protected int resourceReward = 0;

    public delegate void ResourcePickupDelegate(int resourceReward);
    public static event ResourcePickupDelegate resourcePickup;

    protected void Awake() {
        infoText = this.gameObject.GetComponentInChildren<TMP_Text>();

        Collider2D coll = this.GetComponent<Collider2D>();
        coll.isTrigger = true;

        SetResourceReward(15);

    }

    protected override void OnPickup(){

        Debug.Log("Picked up resource: " + resourceReward.ToString());
        if(resourcePickup!=null)
            resourcePickup(resourceReward);

        FMODUnity.RuntimeManager.PlayOneShot("event:/action_phase/Resource pickpu");
        Destroy(this.gameObject);
    }

    public void SetResourceReward(int reward){ 
        resourceReward = reward; 
        infoText.text = resourceReward.ToString();
        }

}
