using UnityEngine;
using System.Collections;

public class CharacterPlayerLock : MonoBehaviour
{

	public int[] JoystickListening = new int[4];

	void Start()
	{
		PlayerController[] detectedControllers = (PlayerController[]) GameObject.FindObjectsOfType(typeof(PlayerController));
		Player tempPlayer;
		for (int i = 0; i < detectedControllers.Length; i++)
		{
			tempPlayer =  new Player();
			tempPlayer.SkinNumber = 0;
			tempPlayer.JoystickNumber = JoystickListening[i];
			detectedControllers[i].Init(tempPlayer);
		}
	}
}
