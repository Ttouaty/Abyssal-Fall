using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Horde_GameRules : AGameRules
{
	public float PercentDistanceFromBorder = 10;
	public float PercentDistanceFromCenter = 20;
	public float MinPopRate = 0.7f;
	public float MaxPopRate = 2f;
	public AnimationCurve PopCurve;

	private float _timeSinceLaunch = 0;

	public override void InitMusic()
	{
		ActiveMusic = SoundManager.Instance.CreateInstance(LevelManager.Instance.CurrentArenaConfig.DMMusicKeys[MatchDuration._valueIndex]);
		if (ActiveMusic == null)
			Debug.LogError("No music found in LevelManager.Instance.CurrentArenaConfig.DMMusicKeys with index => " + MatchDuration._valueIndex);
		else
			ActiveMusic.start();
	}

	public override void InitGameRules()
	{
		base.InitGameRules();

		if (!_isInSuddenDeath)
		{
			GUIManager.Instance.RunTimer(MatchDuration);
			GUIManager.Instance.Timer.OnCompleteCallback.AddListener(OnTimeOut);
		}
		else
			GUIManager.Instance.StopTimer();
	}

	public override IEnumerator Update_Implementation()
	{
		if (!NetworkServer.active)
			yield break;


		float spawnTimer = 0;
		float activePopRate = 0;
		while(_timeSinceLaunch < MatchDuration)
		{
			_timeSinceLaunch += TimeManager.DeltaTime; // Miam le bon timeManager qui sert a rien :p

			activePopRate = Mathf.Lerp(MinPopRate, MaxPopRate, PopCurve.Evaluate(_timeSinceLaunch / (float)MatchDuration));

			spawnTimer += TimeManager.DeltaTime * activePopRate * Mathf.Max(ServerManager.Instance.RegisteredPlayers.Count * 0.6f,1);

			if (spawnTimer >= 1)
			{
				spawnTimer--;

				GhostBehavior newGhost = GameObjectPool.GetAvailableObject<GhostBehavior>("Ghost");

				//Vector3 randomPos = Random.insideUnitCircle * (ArenaManager.Instance.CurrentMapConfig.MapSize.x * 0.5f + DistanceFromBorder);
				float mapsizeX = ArenaManager.Instance.CurrentMapConfig.MapSize.x;
				float distance = Random.Range(mapsizeX * 0.5f * (PercentDistanceFromCenter / 100), mapsizeX - mapsizeX * 0.5f * (PercentDistanceFromBorder/ 100) );
				Vector3 randomPos = Quaternion.Euler(0,Random.Range(0,360), 0) * new Vector3(distance,0,0);
				
				newGhost.transform.position = ArenaManager.Instance.transform.position + randomPos + (ArenaManager.Instance.TileScale + 0.5f)* Vector3.up;
				newGhost.Init(FindObjectsOfType<PlayerController>());
			}

			yield return null;
		}
	}

	private void OnTimeOut()
	{
		if (NetworkServer.active)
		{
			int i;
			int winnerId = 0;
			Player winnerPlayer = ServerManager.Instance.RegisteredPlayers[winnerId];

			for (i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; ++i)
			{
				Player currentPlayer = ServerManager.Instance.RegisteredPlayers[i];
				if (currentPlayer != null)
				{
					currentPlayer.Controller.RpcFreeze();
					if (currentPlayer.Score > winnerPlayer.Score)
					{
						winnerId = i;
						winnerPlayer = currentPlayer;
					}
				}
			}
			GameManager.Instance.OnRoundEndServer.Invoke(winnerPlayer);
		}
	}

	public override void OnPlayerWin_Listener(Player winner)
	{
		base.OnPlayerWin_Listener(winner);

		if(NetworkServer.active)
		{
			GhostBehavior[] ghostsFound = FindObjectsOfType<GhostBehavior>();
			for (int i = 0; i < ghostsFound.Length; i++)
			{
				ghostsFound[i].Kill(ghostsFound[i].transform.position - ArenaManager.Instance.transform.position + ArenaManager.Instance.Position);
			}
		}

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnTimeOut);
	}
}
