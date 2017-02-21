﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayerLock : MonoBehaviour
{
	public Player playerPrefab;
	[Space]
	public PlayerController[] Characters = new PlayerController[4]; 
	public int[] JoystickListening = new int[4];
	IEnumerator Start()
	{
		yield return new WaitUntil(()=> ServerManager.Instance != null);
		yield return new WaitUntil(() => ServerManager.Instance.externalIP != null);

		ServerManager.Instance.StartHostAll("Debug AF", 4, true);
		//NetworkClient tempClient = ServerManager.Instance.StartHost();
		ServerManager.Instance.IsDebug = true;

		Player tempPlayer;
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (!Characters[i].gameObject.activeInHierarchy)
				continue;
			ServerManager.Instance.TryToAddPlayer();
			yield return new WaitUntil(() => Player.PlayerList.Length > i);
			tempPlayer = Player.PlayerList[0];
			//tempPlayer = Instantiate(playerPrefab.gameObject).GetComponent<Player>();
			//tempPlayer.SkinNumber = 0;
			tempPlayer.JoystickNumber = JoystickListening[i];
			//ClientScene.AddPlayer(tempClient.connection, (short)i, );
			//NetworkServer.AddPlayerForConnection(tempClient.connection, tempPlayer.gameObject, (short)i);
			//NetworkServer.Spawn(tempPlayer.gameObject);
			NetworkServer.SpawnWithClientAuthority(Characters[i].gameObject, tempPlayer.gameObject);
			Characters[i]._isInDebugMode = true;
			Characters[i].Init(tempPlayer.gameObject);
		}


	}

	void Update()
	{
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (Characters[i].gameObject.activeInHierarchy)
				Characters[i]._playerRef.JoystickNumber = JoystickListening[i];
		}
	}
}
