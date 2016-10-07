using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPlayerLock : MonoBehaviour
{

	public PlayerController[] Characters = new PlayerController[4]; 
	public int[] JoystickListening = new int[4];
	void Start()
	{
		Player tempPlayer;
		for (int i = 0; i < Characters.Length; i++)
		{
			if (Characters[i] == null)
				continue;
			if (!Characters[i].gameObject.activeInHierarchy)
				continue;
				tempPlayer =  new Player();
			tempPlayer.SkinNumber = 0;
			tempPlayer.JoystickNumber = JoystickListening[i];
			Characters[i].Init(tempPlayer);
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
