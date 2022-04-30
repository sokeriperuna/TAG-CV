using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour{

    public int damage = 10;
    public float initialSpeed = 5;
    public int initialPiercing = 0;
    protected int dmg;
    
    protected float speed;
    protected int piercing = 0;
    private int collisions = 0;

    // Start is called before the first frame update
    void Awake() {
        dmg = damage;
        speed = initialSpeed;
        piercing = initialPiercing;
    }

    // Update is called once per frame
    void FixedUpdate() {
        transform.position = transform.position + transform.right*0.05f * speed;
        
    }

    private void OnCollisionEnter2D(Collision2D other) {
        //Debug.Log("Hit");
        var o = other.gameObject.GetComponent<Entity>();
        //Debug.Log(o);
        if(o != null) {
            o.DoDamage(dmg);
            if(collisions == piercing) {
                Destroy(this.gameObject);
            }
            collisions++;
        }
    }
    
    void OnBecameInvisible() {
         Destroy(this.gameObject);
    }

    
    public int Dmg{get{ return dmg;} set { dmg = value;}}
    public float Speed{get{ return speed;} set {speed = value;}}
    public int Piercing{get{return piercing;} set {piercing = value;}}
}
