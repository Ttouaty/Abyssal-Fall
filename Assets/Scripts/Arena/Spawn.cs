using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;
	private GameObject _spriteId;

	void Start ()
	{
		Tile tileComponent = GetComponent<Tile>();
		if(tileComponent != null)
		{
			tileComponent.enabled = false;
		}
		GetComponent<MeshRenderer>().material.color = Color.green;
	}

	public void SpawnPlayer (PlayerController player)
	{
		_player = player.gameObject;
		_player.transform.position = transform.position + Vector3.up;
		_player.transform.LookAt(new Vector3(0, _player.transform.position.y, 0));
	}
}
