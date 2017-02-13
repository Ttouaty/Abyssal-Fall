using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[Flags]
public enum NetStatus
{
	Disconnected = 1,
	IsServer = 2,
	IsClient = 4
}

public class NetworkStatusCondition : MonoBehaviour
{
	public NetStatus RequiredNetStatus = NetStatus.Disconnected;
	public GameObject[] TargetObjects = new GameObject[0];
	public bool ActiveState = true;
	public bool ForceOpposite = true;

	private NetStatus _activeNetStatus = NetStatus.Disconnected;
	private bool _affectedState;

	void Update()
	{
		ProcessNetStatus();
		if((_activeNetStatus & RequiredNetStatus) != 0)
		{
			for (int i = 0; i < TargetObjects.Length; i++)
			{
				if(TargetObjects[i] != null)
					TargetObjects[i].SetActive(ActiveState);
			}
		}
		else if(ForceOpposite)
		{
			for (int i = 0; i < TargetObjects.Length; i++)
			{
				if(TargetObjects[i] != null)
					TargetObjects[i].SetActive(!ActiveState);
			}
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
