using UnityEngine;
using System.Collections;
using UnityEngine.Networking.Match;
using UnityEngine.Networking;

public class CharacterSelectPanel : MenuPanelNew {

	public override void Open()
	{
		ServerManager.Instance.IsInLobby = true;
		base.Open();
	}

	public override void Return()
	{
		ServerManager.Instance.IsInLobby = true;
		base.ReturnToPreviousPanel(false);
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
		Debug.Log("Match is listed success => "+success+" / info => "+info);
	}

	public void ForceResetNetwork()
	{
		Debug.Log("Resetting network from CharacterSlot");
		ServerManager.Instance.ResetNetwork();
	}

	void OnEnable()
	{
		if (NetworkServer.active)
			ServerManager.Instance.matchMaker.SetMatchAttributes(ServerManager.Instance.matchID, true, 0, OnMatchIslistedCallback);
	}
}
