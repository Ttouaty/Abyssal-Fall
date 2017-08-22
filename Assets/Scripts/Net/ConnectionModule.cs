using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class ConnectionModule : MonoBehaviour
{
	public InputField TargetIPField;

	public UnityEventString OnFailedConnection;
	public UnityEventString OnSuccess;

	[HideInInspector]
	public bool IsConnecting = false;

	void Start()
	{
		OnSuccess.AddListener(OnSuccessCallBack);
		OnFailedConnection.AddListener(OnFailedConnectionCallBack);
	}

	public void TestListMatches()
	{
		NetworkMatch matchMaker = gameObject.AddComponent<NetworkMatch>();
		matchMaker.ListMatches(0, 50, "AbyssalFall-", true, 0, 0, RandomMatchReturn);
	}

	void RandomMatchReturn(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		if(success)
		{
			Debug.Log("info => "+extendedInfo);
			Debug.Log("match founds => " + matchList.Count);

			for (int i = 0; i < matchList.Count; i++)
			{
				Debug.Log("match adress => "+matchList[i].name);
			}
		}
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
		if (IsConnecting)
			return;
		Debug.Log("looking for games with gameType: AbyssalFall-" + Code.ToLower());
		IsConnecting = true;
		ServerManager.Instance.ConnectToMatch(Code.ToLower());
	}

	void OnSuccessCallBack(string Code)
	{
		IsConnecting = false;
		MenuManager.Instance.GetComponentInChildren<TextIP>(true).SetText(Code.ToLower());
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(ShowCharacterSelectCoroutine());
	}

	IEnumerator ShowCharacterSelectCoroutine()
	{
		yield return new WaitUntil(() => Player.LocalPlayer != null);
		MenuPanelNew.PanelRefs["CharacterSelect"].Open();
	}

	void OnFailedConnectionCallBack(string Code)
	{
		IsConnecting = false;
		Network.Disconnect();
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server: " + error);
	}

}

[Serializable]
public class UnityEventString: UnityEvent<string> { }