using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TAG.utility;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/DifficultyPreset", order = 3)]
public class DifficultyPreset : ScriptableObject
{
    public DifficultyPresetParameters parameters;
    public SpawnFormation[] formations;
    public SpawnElement[] coreElements;
    public SpawnElement[] satelliteElements;
}
