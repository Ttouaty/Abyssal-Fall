using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;

	[HideInInspector]
	public ArenaGenerator ArenaGeneratorRef;

	public GameObject SpawnPlayer (int playerId, GameObject playerRef)
	{
		_player = (GameObject) Instantiate(playerRef, transform.position + Vector3.up, Quaternion.identity);
		_player.name = "Player_" + playerId;
		_player.GetComponent<PlayerController>().enabled = false;
		return _player;
	}

	public GameObject ActivatePlayer ()
	{
		_player.GetComponent<PlayerController>().enabled = true;
		return _player;
	}
}
