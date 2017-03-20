using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnlineJoinPanel : ButtonPanelNew
{
	public GameObject BgConnection;

	protected void Update()
	{
		if (_parentMenu.ActiveButtonPanel != this || MenuPanelNew.InputEnabled)
			return;

		if(InputManager.GetButtonDown(InputEnum.Select))
		{
			GetComponentInChildren<PasteClipboard>().Paste();
			MessageManager.Log("Pasted Clipboard");
		}

		if(InputManager.GetButtonDown(InputEnum.A))
		{
			BgConnection.SetActive(true);
			BgConnection.GetComponent<ConnectionModule>().Connect();
			Close();
		}
	}
}
