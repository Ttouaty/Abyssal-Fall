using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking;

public class CharacterSelectPanel : MenuPanelNew {

	private bool matchIsListed= true;

	protected override void Update()
	{
		base.Update();

		if(NetworkServer.active)
		{
			if(ServerManager.Instance.RegisteredPlayers.Count >= 4 && matchIsListed)
			{
				if (ServerManager.Instance.matchMaker == null)
					return;
				ServerManager.Instance.matchMaker.SetMatchAttributes(ServerManager.Instance.matchID, false, 0, OnMatchIslistedCallback);
				Debug.Log("Unlisting match 4 players connected !");
				matchIsListed = false;
			}
		}
	}


	public override void Open()
	{
		ServerManager.Instance.IsInLobby = true;

		for (int i = 0; i < Player.PlayerList.Length; i++)
		{
			if (Player.PlayerList[i] == null)
				continue;

			if (Player.PlayerList[i].isLocalPlayer)
				MenuManager.Instance.SetControllerInUse(Player.PlayerList[i].JoystickNumber, true);
		}

		base.Open();
	}

	public override void Return()
	{
		base.ReturnToPreviousPanel(false);
		ServerManager.Instance.IsInLobby = false;
	}

	public override void Close()
	{
		if(NetworkServer.active)
			ServerManager.Instance.matchMaker.SetMatchAttributes(ServerManager.Instance.matchID, false, 0, OnMatchIslistedCallback);
		ServerManager.Instance.IsInLobby = false;
		base.Close();
	}

	private void OnMatchIslistedCallback(bool success, string info)
	{
		Debug.Log("Match is listed as "+ matchIsListed + " with success => "+success+" / info => "+info);
		if (!success)
			matchIsListed = !matchIsListed;
	}

	public void ForceResetNetwork()
	{
		Debug.Log("Resetting network from CharacterSlot");
		ServerManager.Instance.ResetNetwork();
	}

	void OnEnable()
	{
		if (NetworkServer.active)
		{
			ServerManager.Instance.matchMaker.SetMatchAttributes(ServerManager.Instance.matchID, true, 0, OnMatchIslistedCallback);
			matchIsListed = true;
		}
	}
}
