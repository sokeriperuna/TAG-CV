using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pickup : MonoBehaviour
{
    
    abstract protected void OnPickup();

    protected void OnTriggerEnter2D(Collider2D other) {

        if(other.CompareTag("Attacker"))
            OnPickup();

    }
}
