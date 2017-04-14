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

		if (ServerManager.Instance.FacilitatorStatus == FacilitatorConnectionStatus.Failed)
		{
			_parentMenu.ActiveButtonPanel = null;
			MessageManager.Log("Could not connect to our facilitator server.\nPlease check your internet connection,\nbut it is probably a problem on our end.", 10);
			Return();
		}

		if (--frameBuffer > 0) // ooooh oui je l'aime ma belle truelle !
			return;

		if(InputManager.GetButtonDown(InputEnum.Select))
		{
			GetComponentInChildren<PasteClipboard>().Paste();
			MessageManager.Log("Pasted Clipboard");
		}

		if(InputManager.GetButtonDown(InputEnum.A))
		{

			string code = GetComponentInChildren<InputField>().text;
			if (code == ServerManager.Instance.GameId)
			{
				Debug.LogWarning("Detected Auto connect, aborting");
				MessageManager.Log("Could not connect to Host: Can't connect to your own game :/");
				return;
			}

			if (code.Length != 8)
			{
				if (code.Length == 0)
				{
					Debug.LogWarning("Game Id is empty");
					MessageManager.Log("Could not connect to Host: No Game Id found.");
				}
				else
				{
					Debug.LogWarning("Game Id should be 8 characters long.");
					MessageManager.Log("Could not connect to Host: Game Id should be 8 characters long.");
				}
				return;
			}

			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				Debug.LogWarning("No internet connection found");
				MessageManager.Log("Could not connect to Host: No Internet connection found.");
				return;
			}

			BgConnection.SetActive(true);
			BgConnection.GetComponent<ConnectionModule>().Connect();
			Close();
		}
	}
}
