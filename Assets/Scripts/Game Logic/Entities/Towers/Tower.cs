using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Entity
{
    public int deathResourceReward = 5;

    public Vector2 AimDir{get { return currentTarget!=null?
                                       (Vector2)(currentTarget.transform.position-transform.position):
                                        Vector2.zero;}}

}
