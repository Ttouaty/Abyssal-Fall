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
		if(ServerManager.Instance.ExternalIp.Length == 0)
		{
			Debug.LogWarning("No internet connection found");
			OnFailedConnection.Invoke("Could not connect to Host: No Internet found.");
			return;
		}

		Debug.Log("looking for games with gameType: "+ "AbyssalFall-" + Code);
		Debug.Log("Setting MenuManager joystick buffer");

		MenuManager.Instance.LocalJoystickBuffer.Add(0);
		if (InputManager.AnyButtonDown(true) != -1)
			MenuManager.Instance.LocalJoystickBuffer.Add(InputManager.AnyButtonDown(true));

		ServerManager.Instance.ConnectToMatch(Code);
	}

	void OnSuccessCallBack(string Code)
	{
		FindObjectOfType<TextIP>().GetComponent<InputField>().readOnly = false;
		FindObjectOfType<TextIP>().GetComponent<InputField>().text = Code;
		FindObjectOfType<TextIP>().GetComponent<InputField>().readOnly = true;

		//CharacterSelectWheel[] wheels = MenuManager.Instance._characterSlotsContainerRef.GetComponentsInChildren<CharacterSelectWheel>(true);
		//for (int i = 0; i < wheels.Length; i++)
		//{
		//	Debug.LogError("destroy wheel "+ wheels[i].gameObject.name);
		//	//Destroy(wheels[i].gameObject);
		//}
	}

	void OnFailedToConnect(NetworkConnectionError error)
	{
		OnFailedConnection.Invoke("Could not connect to server: " + error);
	}

}

[Serializable]
public class UnityEventString: UnityEvent<string> { }