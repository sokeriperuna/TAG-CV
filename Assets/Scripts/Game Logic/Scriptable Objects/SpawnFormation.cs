using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TAG.utility;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnFormation", order = 3)]
public class SpawnFormation : ScriptableObject
{
    public FORMATION_TYPE type;
    public ORIENTATION_TYPE orientation;
    public int numberOfSatellites;
    public float arcRadius;
    public float arcLength = 360f;
    public float costMultiplier = 1f;
    public int weight = 1;
}
