using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Relic : MonoBehaviour
{
	
	private Rigidbody _rigidBRef;
	private Vector3 _startPosition;

	void Start()
	{
		_rigidBRef = GetComponent<Rigidbody>();
		_startPosition = transform.position;
		CameraManager.Instance.AddTargetToTrack(transform);
	}

	public void Eject(Vector3 targetVelocity)
	{
		_rigidBRef.velocity = targetVelocity;
	}

	public void Respawn()
	{
		if(ArenaManager.Instance != null)
		{
			IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t != null).Where((Tile t) => t.Obstacle == null && t.CanFall/* && !t.IsSpawn*/);
			List<Tile> tiles = new List<Tile>(tilesEnumerator);

			transform.position = tiles.ShiftRandomElement().transform.position + Vector3.up * 50;
			Eject(Vector3.down * 3);
		}
		else
		{
			transform.position = _startPosition + Vector3.up * 50;
			Eject(Vector3.down * 3); 
		}
	}
}
