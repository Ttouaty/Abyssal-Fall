using UnityEngine;
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

		ServerManager.Instance.IsDebug = true;
		ServerManager.Instance.StartHostAll("Debug AF", 4, true);
		//NetworkClient tempClient = ServerManager.Instance.StartHost();

		Player tempPlayer;
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (!Characters[i].gameObject.activeInHierarchy)
				continue;

			int tempPlayerListLength = Player.PlayerList.Length;

			if (i != 0)
			{
				ServerManager.Instance.TryToAddPlayer();
				yield return new WaitUntil(() => Player.PlayerList.Length > tempPlayerListLength);
			}

			tempPlayer = Player.PlayerList[0];

			tempPlayer.JoystickNumber = JoystickListening[i];

			NetworkServer.SpawnWithClientAuthority(Characters[i].gameObject, tempPlayer.gameObject);
			Characters[i]._isInDebugMode = true;
			Characters[i].gameObject.SetActive(true);
			Characters[i].Init(tempPlayer.gameObject);
			Characters[i].RpcUnFreeze();
		}


	}

	void Update()
	{
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (Characters[i].gameObject.activeInHierarchy)
			{
				if(Characters[i]._playerRef != null)
					Characters[i]._playerRef.JoystickNumber = JoystickListening[i];
			}
		}
	}
}
