using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour
{
	private GameObject _player;
	private GameObject _spriteId;
	private PlayerController _controller;

	void Start ()
	{
        Tile tileComponent = GetComponent<Tile>();
        if(tileComponent != null)
        {
            tileComponent.enabled = false;
        }
        GetComponent<MeshRenderer>().material.color = Color.green;
	}

    void OnDestroy ()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }

	public void SpawnPlayer (GameObject player)
	{
        _player = player;
        _player.transform.position = transform.position + Vector3.up;
        _player.transform.LookAt(new Vector3(0, _player.transform.position.y, 0));
    }

	public GameObject ActivatePlayer ()
	{
		_controller.enabled = true;
		return _player;
	}

	public void Destroy ()
	{
		if(_player != null)
		{
			Destroy(_player);
		}
		DestroyId();
	}

	public void DestroyId ()
	{
		if(_spriteId != null)
		{
			Destroy(_spriteId);
		}
	}
}
