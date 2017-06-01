using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DeathMatch_GameRules : AGameRules
{
	public override void InitGameRules ()
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

	private void OnTimeOut()
	{
		if(NetworkServer.active)
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

	public override void OnPlayerWin_Listener (Player winner)
	{
		base.OnPlayerWin_Listener(winner);

		GUIManager.Instance.Timer.OnCompleteCallback.RemoveListener(OnTimeOut);
	}
}
