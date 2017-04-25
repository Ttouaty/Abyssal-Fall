using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewModeConfiguration", menuName = "Abyssal Fall/Mode Configuration", order = 1)]
public class ModeConfiguration_SO : ScriptableObject
{
	public IntRule ScoreToWin;
	public IntRule NumberOfCharactersRequired;
	public IntRule MatchDuration;

	public IntRule TileRegerationTime;

	public IntRule PointsPerKill;
	public IntRule PointsPerSuicide;
	public IntRule TimeBeforeSuicide;

	public IntRule TimeBeforeAutoDestruction;
	public IntRule IntervalAutoDestruction;

	public BoolRule CanFallenTilesRespawn;
	public BoolRule IsMatchRoundBased;
	public BoolRule CanPlayerRespawn;
	public BoolRule ArenaAutoDestruction;
	public BoolRule RandomArena;
	//Defined in the gameObject
	//public List<GameObject> RespawnZones = new List<GameObject>();

	//public BehaviourConfiguration[] Behaviours = new BehaviourConfiguration[] { };

	//public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();
}
