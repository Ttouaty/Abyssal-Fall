using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class OnlineJoinPanel : ButtonPanelNew
{
	public GameObject BgConnection;
	private int frameBuffer = 0;

	public override void Open()
	{
		base.Open();
		frameBuffer = 3; // regarde moi cette belle truelle :]
	}

	protected void Update()
	{
		if (_parentMenu.ActiveButtonPanel != this || !MenuPanelNew.InputEnabled)
			return;


		if (ServerManager.Instance.FacilitatorStatus != FacilitatorConnectionStatus.Connected)
		{
			_parentMenu.ActiveButtonPanel = null;
			MessageManager.Log("Could not connect to our facilitator server.\n(You can still play locally)", 7);
			Return();
		}

		if (--frameBuffer > 0) // ooooh oui je l'aime ma belle truelle !
			return;

		if(InputManager.GetButtonDown(InputEnum.Select))
		{
			GetComponentInChildren<PasteClipboard>().Paste();
			MessageManager.Log("Pasted Clipboard");
		}

		if(InputManager.GetButtonDown(InputEnum.A) && !BgConnection.GetComponent<ConnectionModule>().IsConnecting)
		{
			BgConnection.GetComponent<ConnectionModule>().ConnectToPrivateGame();
		}
	}

	public void SetBGConnectionActive(bool isActive)
	{
		BgConnection.SetActive(isActive);
	}

	public override void Return()
	{
		if (BgConnection.GetComponent<ConnectionModule>().IsConnecting)
		{
			BgConnection.GetComponent<ConnectionModule>().OnFailedConnection.Invoke("Canceled connection");
		}
		else
		{
			SetBGConnectionActive(false);
			base.Return();
		}
	}
}
