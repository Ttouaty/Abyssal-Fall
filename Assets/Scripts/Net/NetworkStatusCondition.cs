using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[Flags]
public enum NetStatus
{
	Disconnected,
	IsServer,
	IsClient
}

public class NetworkStatusCondition : MonoBehaviour
{
	public NetStatus RequiredNetStatus = NetStatus.Disconnected;
	public GameObject[] TargetObjects = new GameObject[0];
	public bool ActiveState = true;

	private NetStatus _activeNetStatus = NetStatus.Disconnected;
	private bool _affectedState;

	void Update()
	{
		ProcessNetStatus();
		_affectedState = (_activeNetStatus & RequiredNetStatus) != 0 ? ActiveState : !ActiveState;

		for (int i = 0; i < TargetObjects.Length; i++)
		{
			TargetObjects[i].SetActive(_affectedState);
		}
	}

	void ProcessNetStatus()
	{
		_activeNetStatus = NetStatus.Disconnected;
		if (NetworkClient.active)
			_activeNetStatus |= NetStatus.IsClient;
		if(NetworkServer.active)
			_activeNetStatus |= NetStatus.IsServer;

	}
}
