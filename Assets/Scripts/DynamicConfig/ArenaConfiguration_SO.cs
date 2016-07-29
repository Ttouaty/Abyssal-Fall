using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArenaConfiguration", menuName = "Arena/ArenaConfiguration", order = 1)]
public class ArenaConfiguration_SO : ScriptableObject
{
    public GameObject Ground;
    public List<PoolConfiguration> OtherAssetsToLoad;
    public string BackgroundLevel;
    [Range(16, 64)] public int ArenaSize = 32;
    public float TileScale = 1;
    public Dictionary<string, ABehavior> Behaviors;
}