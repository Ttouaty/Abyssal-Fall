﻿using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;
	private PlayerController _controller;

	void Start ()
	{
		GetComponent<Tile>().enabled = false;
	}

	public GameObject SpawnPlayer (int playerId, GameObject playerRef, Material materialRef, Material idMaterialRef)
	{
		_player = (GameObject) Instantiate(playerRef, transform.position + Vector3.up, Quaternion.identity);
		_player.name = "Player_" + playerId;

		Transform child = _player.transform.GetChild(0);
		child.GetComponent<MeshRenderer>().material = materialRef;
		_controller = _player.GetComponent<PlayerController>();
		_controller.enabled = false;
		_controller.PlayerNumber = playerId + 1;

		GameObject spriteId = GameObject.CreatePrimitive(PrimitiveType.Quad);
		spriteId.name = "SpriteId_" + playerId;
		spriteId.GetComponent<MeshRenderer>().material = idMaterialRef;
		SpriteID id = spriteId.AddComponent<SpriteID>();
		id.Target = _player.transform;

		return _player;
	}

	public GameObject ActivatePlayer ()
	{
		_controller.enabled = true;
		return _player;
	}
}
