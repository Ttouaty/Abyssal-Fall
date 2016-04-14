using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;
	private GameObject _spriteId;
	private PlayerController _controller;

	void Start ()
	{
		GetComponent<Tile>().enabled = false;
	}

	public GameObject SpawnPlayer (int playerId, GameObject playerRef, Material materialRef, Material idMaterialRef)
	{
		_player = (GameObject) Instantiate(playerRef, transform.position + Vector3.up, Quaternion.identity);
		_player.name = "Player_" + playerId;

		_controller = _player.GetComponent<PlayerController>();
		_controller.enabled = false;
		_controller.PlayerNumber = playerId + 1;

		_player.GetComponent<PlayerChangeMaterial>().Target.material = materialRef;

		_spriteId = GameObject.CreatePrimitive(PrimitiveType.Quad);
		_spriteId.name = "SpriteId_" + playerId;
		_spriteId.GetComponent<MeshRenderer>().material = idMaterialRef;
		SpriteID id = _spriteId.AddComponent<SpriteID>();
		id.Target = _player.transform;

		return _player;
	}

	public GameObject ActivatePlayer ()
	{
		_controller.enabled = true;
		return _player;
	}

	public void Destroy ()
	{
		Destroy(_player);
	}

	public void DestroyId ()
	{
		Destroy(_spriteId);
	}
}
