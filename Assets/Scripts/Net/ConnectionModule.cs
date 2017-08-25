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

	private OnlineJoinPanel __internalOnlineJoinPanelRef;
	public OnlineJoinPanel OnlinePanel
	{
		get
		{
			if(__internalOnlineJoinPanelRef == null)
				__internalOnlineJoinPanelRef = transform.parent.GetComponentInChildren<OnlineJoinPanel>(true);
			return __internalOnlineJoinPanelRef;
		}
	}


	void Start()
	{
		OnSuccess.AddListener(OnSuccessCallBack);
		OnFailedConnection.AddListener(OnFailedConnectionCallBack);
	}

	public void ConnectToRandomMatch()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			Debug.LogWarning("No internet connection found");
			MessageManager.Log("Error: No Internet connection found.");
			return;
		}

		if (ServerManager.Instance.matchMaker == null) ServerManager.Instance.matchMaker = ServerManager.Instance.gameObject.AddComponent<NetworkMatch>();

		ServerManager.Instance.matchMaker.ListMatches(0, 50, "-AbyssalFall-Public", true, 0, 0, RandomMatchReturn);
		MessageManager.Log("Retreiving random matches list...",5);


		OnlinePanel.SetBGConnectionActive(true);
		OnlinePanel.Close();
	}

	private void RandomMatchReturn(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		if (success)
		{
			if(matchList.Count != 0)
			{
				Debug.Log("Received match list! " + matchList.Count + " games found. (max 50)");
				MessageManager.Log("Received match list! "+ matchList.Count+" games found. (max 50)", 5);
				Connect(matchList.ShiftRandomElement());
			}
			else
				OnFailedConnection.Invoke("Failed to find any public matches... sorry :(");
		}
		else
			OnFailedConnection.Invoke("Failed to find any public matches... sorry :(");//ouai doublon et je te merde
	}

	public void ConnectToPrivateGame()
	{
		if (TargetIPField == null)
		{
			Debug.LogWarning("No TargetIPField found, Use Connect(string fullCode); if you want to connect to a specified game.");
			OnFailedConnection.Invoke("Error No TargetIPField found");
			return;
		}

		string code = TargetIPField.text;

		//if (code == ServerManager.Instance.GameId)
		//{
		//	Debug.LogWarning("Detected Auto connect, aborting");
		//	MessageManager.Log("Could not connect to Host: Can't connect to your own game :/");
		//	return;
		//}

		if (code.Length != 6)
		{
			if (code.Length == 0)
			{
				Debug.LogWarning("Game Id is empty");
				MessageManager.Log("Error: No Game Id found.");
			}
			else
			{
				Debug.LogWarning("Game Id should be 6 characters long.");
				MessageManager.Log("Error: Game Id should be 6 characters long.");
			}
			return;
		}

		OnlinePanel.SetBGConnectionActive(true);
		OnlinePanel.Close();
		Connect(TargetIPField.text+"-AbyssalFall");
	}

	public void Connect(string FullString)
	{
		if (IsConnecting)
			return;

		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			Debug.LogWarning("No internet connection found");
			MessageManager.Log("Error: No Internet connection found.");
			return;
		}

		Debug.Log("looking for games with gameType: " + FullString);
		IsConnecting = true;
		ServerManager.Instance.ConnectToMatch(FullString);
	}

	public void Connect(MatchInfoSnapshot match)
	{
		if (IsConnecting)
			return;

		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			Debug.LogWarning("No internet connection found");
			MessageManager.Log("Error: No Internet connection found.");
			return;
		}

		IsConnecting = true;
		ServerManager.Instance.ConnectToTargetMatch(match);
	}

	void OnSuccessCallBack(string Code)
	{
		IsConnecting = false;
		MenuManager.Instance.GetComponentInChildren<TextIP>(true).SetText(Code);
		MainManager.Instance.GetComponent<MonoBehaviour>().StartCoroutine(ShowCharacterSelectCoroutine());
		OnlinePanel.SetBGConnectionActive(false);
	}

	IEnumerator ShowCharacterSelectCoroutine()
	{
		yield return new WaitUntil(() => Player.LocalPlayer != null);
		MenuPanelNew.PanelRefs["CharacterSelect"].Open();
	}

	void OnFailedConnectionCallBack(string Code)
	{
		IsConnecting = false;
		ServerManager.Instance.ResetNetwork();
		OnlinePanel.SetBGConnectionActive(false);
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server: " + error);
	}

}

[Serializable]
public class UnityEventString: UnityEvent<string> { }