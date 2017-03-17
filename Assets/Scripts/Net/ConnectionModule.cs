using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;

public class ConnectionModule : MonoBehaviour
{
	public InputField TargetIPField;

	public UnityEventString OnFailedConnection;
	public UnityEventString OnSuccess;

	void Start()
	{
		OnSuccess.AddListener(OnSuccessCallBack);
	}

	public void Connect()
	{
		if(TargetIPField == null)
		{
			Debug.LogWarning("No TargetIPField found, Use Connect(string IPandPort); if you want to connect to a specified game.");
			OnFailedConnection.Invoke("No TargetIPField found, Use Connect(string IPandPort); if you want to connect to a specified game.");
			return;
		}

		Connect(TargetIPField.text);
	}

	public void Connect(string Code)
	{
		if(Application.internetReachability == NetworkReachability.NotReachable)
		{
			Debug.LogWarning("No internet connection found");
			OnFailedConnection.Invoke("Could not connect to Host: No Internet connection found.");
			return;
		}

		if(Code == ServerManager.Instance.GameId)
		{
			Debug.LogWarning("Detected Auto connect, aborting");
			OnFailedConnection.Invoke("Could not connect to Host: Can't connect to your own game :/");
			return;
		}

		if (Code.Length != 8)
		{
			if (Code.Length == 0)
			{
				Debug.LogWarning("Game Id string is empty");
				OnFailedConnection.Invoke("Could not connect to Host: No Game Id found.");
			}
			else
			{
				Debug.LogWarning("Game Id string is not 8 characters");
				OnFailedConnection.Invoke("Could not connect to Host: Game Id should be 8 characters long.");
			}
			return;
		}
		Debug.Log("looking for games with gameType: AbyssalFall-" + Code.ToLower());

		ServerManager.Instance.ConnectToMatch(Code.ToLower());
	}

	void OnSuccessCallBack(string Code)
	{
		MenuManager.Instance.GetComponentInChildren<TextIP>(true).SetText(Code.ToLower());
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(ShowCharacterSelectCoroutine());
	}

	IEnumerator ShowCharacterSelectCoroutine()
	{
		yield return new WaitUntil(() => Player.LocalPlayer != null);
		MenuPanelNew.PanelRefs["CharacterSelect"].Open();
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server: " + error);
	}

}

[Serializable]
public class UnityEventString: UnityEvent<string> { }