using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;

	void Start ()
	{
		GetComponent<MeshRenderer>().material.color = Color.red;
	}

	public void SpawnPlayer (int playerId)
	{
		_player = (GameObject) Instantiate(GameManager.instance.PlayersRefs[playerId], transform.position + Vector3.up, Quaternion.identity);
		_player.GetComponent<PlayerController>().enabled = false;
	}

	public void ActivatePlayer ()
	{
		_player.GetComponent<PlayerController>().enabled = true;
	}
}
