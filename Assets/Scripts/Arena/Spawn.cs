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
			tileComponent.enabled = false; //commenté pour débug
		}
		GetComponentInChildren<MeshRenderer>().material.color = Color.green;
	}

	public void SpawnPlayer (PlayerController player)
	{
		_player = player.gameObject;
		_player.transform.position = transform.position + Vector3.up;
		_player.transform.LookAt(transform.parent.position.SetAxis(Axis.y, _player.transform.position.y));
	}
}
