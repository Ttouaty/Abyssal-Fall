using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AGameRules : MonoBehaviour 
{
	public bool IsMatchRoundBased = true;
	public int NumberOfRounds = 5;
	public int MatchDuration = 180;

	public bool CanFalledTilesRespawn = false;
	public int TileRegerationTime = 0;

	public int PointsPerKill = 1;
	public int PointsPerSuicide = 0;
	public int TimeBeforeSuicide = 5;

	public bool CanPlayerRespawn = false;
	public List<GameObject> RespawnZones = new List<GameObject>();

	public BehaviourConfiguration[] Behaviours = new BehaviourConfiguration[] { };

	public List<PoolConfiguration> AdditionalPoolsToLoad = new List<PoolConfiguration>();

	public virtual void InitGameRules () 
	{
		if(IsMatchRoundBased)
		{
			GUIManager.Instance.RunRoundCount();
		}
		else 
		{
			GUIManager.Instance.RunTimer(MatchDuration);
		}
	}

	public virtual void OnPlayerDeath_Listener (Player player)
	{
		
	}

	public virtual void OnPlayerWin_Listener (Player player)
	{
		
	}
}
