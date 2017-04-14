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