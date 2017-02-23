using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class FreeForAll_GameRules : AGameRules
{
	private Coroutine roundEndCoroutine;

	public override void InitGameRules ()
	{
		base.InitGameRules();
		GUIManager.Instance.RunRoundCount();
	}

	public override void OnPlayerDeath_Listener (Player player, Player killer)
	{
		base.OnPlayerDeath_Listener(player, killer);

		if(NetworkServer.active)
		{
			if (ServerManager.Instance.AlivePlayers.IndexOf(player) >= 0)
			{
				ServerManager.Instance.AlivePlayers.Remove(player);
				// Round won
				if (ServerManager.Instance.AlivePlayers.Count == 1)
				{
					// Invoke win event after 1 sec
					roundEndCoroutine = StartCoroutine(DelayRoundEnd());
				}
			}
			else
			{
				Debug.LogError("DRAW DETECTED !");
				StopCoroutine(roundEndCoroutine);
				GameManager.Instance.OnRoundEndServer.Invoke(null);
			}
		}
	}

	public override void OnRoundEnd_Listener_Server(Player winner)
	{
		// + 1 Point to last alive
		if(winner != null)
			winner.Score++;
		base.OnRoundEnd_Listener_Server(winner);
	}

	private IEnumerator DelayRoundEnd()
	{
		yield return new WaitForSeconds(1);
		if(ServerManager.Instance.AlivePlayers.Count == 0)
			GameManager.Instance.OnRoundEndServer.Invoke(null);
		else
			GameManager.Instance.OnRoundEndServer.Invoke(ServerManager.Instance.AlivePlayers[0]);
	}

	public override void OnRoundEnd_Listener(Player winner)
	{
		base.OnRoundEnd_Listener(winner);

		if (winner == null)
		{
			MessageManager.Log("Draw! No bonus point awarded !");
			return;
		}
	}

	public override void OnPlayerWin_Listener (Player winner)
	{
		
		base.OnPlayerWin_Listener(winner);
		//Player winner = ServerManager.Instance.AlivePlayers[0];
		winner.Controller.Freeze();
		winner.Score++;
		InputManager.AddInputLockTime(1);

		GUIManager.Instance.SetActiveRoundCount(false);
		

	}
}
