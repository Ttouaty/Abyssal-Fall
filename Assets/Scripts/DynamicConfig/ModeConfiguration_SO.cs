using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
    public bool IsMatchRoundBased = true;
    public int NumberOfRounds = 5;
    public int MatchDuration = 180;

    public bool CanFalledTilesRespawn = false;
    public int TileRegerationTime = 0;

    public int PointsPerKill = 0;
    public int PointsPerSuicide = 0;
    public int TimeBeforeSuicide = 5;

    public bool CanPlayerRespawn = false;
    public List<GameObject> RespawnZones = new List<GameObject>();

    public BehaviourConfiguration[] Behaviours = new BehaviourConfiguration[] { };

    public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();
}
