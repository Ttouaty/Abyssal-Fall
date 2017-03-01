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
	}

	public void SpawnPlayer (PlayerController player)
	{
		_player = player.gameObject;
		_player.transform.position = transform.position + Vector3.up * transform.localScale.y * 0.5f + Vector3.up * player.GetComponent<CapsuleCollider>().height * 0.5f * player.transform.localScale.y;
		_player.transform.LookAt(transform.parent.position.SetAxis(Axis.y, _player.transform.position.y));
	}

	public void Colorize(Color targetColor)
	{
		GetComponentInChildren<MeshRenderer>().material.color = targetColor;
	}
}
