using UnityEngine;
using System.Collections;

public class CharacterSelectPanel : MenuPanelNew {

	public override void Open()
	{
		ServerManager.Instance.IsInLobby = true;
		base.Open();
	}

	public override void Close()
	{
		ServerManager.Instance.IsInLobby = false;
		base.Close();
	}

}
