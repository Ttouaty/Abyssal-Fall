using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Relic_GameRules : AGameRules
{
	public override void InitGameRules()
	{
		base.InitGameRules();
		if (!_isInSuddenDeath)
		{
			GUIManager.Instance.RunTimer(MatchDuration);
			GUIManager.Instance.Timer.OnCompleteCallback.AddListener(OnTimeOut);
			if (NetworkServer.active)
			{
				//Spawn Relic
				GameObject newRelic = ServerManager.Instance.SpawnObjectOfType<Relic>(null);
				newRelic.transform.position = ArenaManager.Instance.TilesRoot.position + (Vector3)ArenaManager.Instance.CurrentMapConfig.MapSize * 0.5f + Vector3.up;
			}
		}
		else
			GUIManager.Instance.StopTimer();
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

			//Player.LocalPlayer.RpcOnPlayerWin(winnerPlayer.gameObject);
		}
	}

	public override void OnPlayerWin_Listener(Player winner)
	{
		base.OnPlayerWin_Listener(winner);

		// TODO : Gérer les égalités

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnTimeOut);
	}

	public void UpdatePlayerScore(GameObject playerObject, float timePassed)
	{
		if (!NetworkServer.active)
			return;

		playerObject.GetComponent<Player>().Score += timePassed;
	}
}
