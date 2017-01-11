using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayerLock : MonoBehaviour
{

	public PlayerController[] Characters = new PlayerController[4]; 
	public int[] JoystickListening = new int[4];
	void Start()
	{
		ServerManager.Instance.IsInLobby = true;
		ServerManager.Instance.StartHostAll("Debug AF", 4, false, "osd4fdsfs8f4s98d7f");
		Player tempPlayer;
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (!Characters[i].gameObject.activeInHierarchy)
				continue;
				tempPlayer =  new GameObject("Player n"+i).AddComponent<Player>();
			tempPlayer.SkinNumber = 0;
			tempPlayer.JoystickNumber = JoystickListening[i];
			NetworkServer.Spawn(tempPlayer.gameObject);
			Characters[i].Init(tempPlayer.gameObject);
		}

		ServerManager.Instance.IsInLobby = false;

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
