using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/UpgradeUI", order = 2)]
public class UpgradeUI : ScriptableObject
{
    public Sprite upgradeIcon;
    public string upgradeName;
    [TextArea] public string upgradeDescription;
}
