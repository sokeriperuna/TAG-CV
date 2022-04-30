using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TAG.utility;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnElement", order = 4)]
public class SpawnElement : ScriptableObject
{
    public SPAWN_TYPE type;
    public int cost;
    public float costMultiplier = 1;
    public bool cannotSpawnAlone;
    public GameObject prefab;
    public int weight = 1;
}
