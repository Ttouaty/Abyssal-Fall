using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
	public bool IsMatchRoundBased;
	public IntRule NumberOfRound;
	public IntRule MatchDuration;

	public BoolRule CanFalledTilesRespawn;
	public IntRule TileRegerationTime;

	public IntRule PointsPerKill;
	public IntRule PointsPerSuicide;
	public IntRule TimeBeforeSuicide;

	public bool CanPlayerRespawn;
	public List<GameObject> RespawnZones = new List<GameObject>();

	public BehaviourConfiguration[] Behaviours = new BehaviourConfiguration[] { };

	public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();
}
