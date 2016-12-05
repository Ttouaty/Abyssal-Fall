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
	public UnityEvent OnSuccess;

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

	public void Connect(string IPandPort)
	{
		if(ServerManager.Instance.ExternalIp.Length == 0)
		{
			Debug.LogWarning("No internet connection found");
			OnFailedConnection.Invoke("Could not connect to Host: No Internet found.");
			return;
		}

		string[] fragmentIp = IPandPort.Split(':');
		if(fragmentIp[0].Split('.').Length < 3)
		{
			Debug.LogWarning("invalid IP address");
			OnFailedConnection.Invoke("Wrong Ip and port format, it should look like this: 123.123.123.123");
			return;
		}
		MasterServer.RequestHostList("AbyssalFall-"+ fragmentIp[0]);
		Debug.Log("looking for games with ip: "+ "AbyssalFall-" + fragmentIp[0]);
		StartCoroutine(ConnectionCoroutine());
		//Network.Connect(fragmentIp[0], Convert.ToInt32(fragmentIp[1]));
	}

	IEnumerator ConnectionCoroutine()
	{
		HostData[] tempHostData = MasterServer.PollHostList();
		int tries = 0;
		while (tempHostData.Length == 0 &&  tries < 5)
		{
			tries++;
			Debug.Log("trying to poll results");
			tempHostData = MasterServer.PollHostList();
			yield return new WaitForSeconds(1);
		}

		if(tries >= 5)
		{
			//too many tries, cancelling;

			Debug.Log("too many tries");
			MasterServer.ClearHostList();
			OnFailedToConnect(NetworkConnectionError.ConnectionFailed);
			yield break;
		}
		else
		{
			Debug.Log("Server found ! "+ tempHostData[0].gameType);
			Network.Connect(tempHostData[0].guid);
		}
	}

	void OnConnectedToServer()
	{
		OnSuccess.Invoke();
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server: "+error);
	}
}

[Serializable]
public class UnityEventString: UnityEvent<string> { }