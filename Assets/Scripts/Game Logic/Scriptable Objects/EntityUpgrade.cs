using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UPGRADE_TYPE {
        BASIC, SPECIAL
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/EntityUpgrade", order = 1)]
public class EntityUpgrade : ScriptableObject
{
        [Header("Upgrade overview")]
        public UPGRADE_TYPE type = UPGRADE_TYPE.BASIC;
        public int startingCost = 5;
        public float costGain = 1.3f;

        [Header("Convoy upgrades")]
        public float speedBoost;
        public int flareBoost;
        public int unitBoost;
        public GameObject unitPrefab;

        [Header("Entity upgrades")]
        public int entityHp;
        public int entityRange;
        public float gunFireRate;
        public int bulletSpeed;
        public int bulletDamage;
        public int bulletPiercing;

        public GameObject gun;
        public GameObject bullet;

        public UpgradeUI UIData;
}
