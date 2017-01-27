using UnityEngine;
using System.Collections;

public class CharacterSelectPanel : MenuPanelNew {

	public override void Open()
	{
		base.Open();
		ServerManager.Instance.IsInLobby = true;
	}

	public override void Close()
	{
		base.Close();
		ServerManager.Instance.IsInLobby = false;
	}

}
