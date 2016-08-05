using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewArenaConfiguration", menuName = "Abyssal Fall/Arena Configuration", order = 1)]
public class ArenaConfiguration_SO : ScriptableObject
{
    public GameObject Ground;
    public GameObject Obstacle;
    public List<PoolConfiguration> OtherAssetsToLoad;
    public string BackgroundLevel;
    public Dictionary<string, ABehavior> Behaviors;
}