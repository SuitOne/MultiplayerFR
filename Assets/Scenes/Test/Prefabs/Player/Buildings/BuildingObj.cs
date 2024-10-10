using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainR
{
    [CreateAssetMenu(menuName = "MainR/New Building")]
    public class BuildingObj : ScriptableObject
    {
        public string buildingName;
        public int health;
        public GameObject ghostPrefab;
        public GameObject prefab;
        public int resourceCost;
        public int manpowerCost;
    }
}
