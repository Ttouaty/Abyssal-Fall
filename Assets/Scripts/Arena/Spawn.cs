using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	[HideInInspector]
	public Arena ArenaRef;

	private GameObject _player;

	void Start ()
	{
		GetComponent<MeshRenderer>().material.color = Color.red;
	}

	public void SpawnPlayer (int playerId, GameObject playerRef)
	{
		_player = (GameObject) Instantiate(playerRef, transform.position + Vector3.up, Quaternion.identity);
		_player.name = "PLayer_" + playerId;
		_player.GetComponent<PlayerController>().enabled = false;
	}

	public void ActivatePlayer ()
	{
		_player.GetComponent<PlayerController>().enabled = true;
	}
}
