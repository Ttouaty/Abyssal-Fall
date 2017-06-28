using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Relic : MonoBehaviour
{
	[SerializeField]
	private GameObject NoSpecialDiv;

	private Rigidbody _rigidBRef;
	private Vector3 _startPosition;
	[HideInInspector]
	public bool Grabbed = false;

	void Start()
	{
		_rigidBRef = GetComponent<Rigidbody>();
		_startPosition = transform.position;
		CameraManager.Instance.AddTargetToTrack(transform);
	}

	void Update()
	{
		NoSpecialDiv.SetActive(Grabbed);
		_rigidBRef.velocity = Vector3.ClampMagnitude(_rigidBRef.velocity + Physics.gravity * Time.deltaTime * 5, 75);
	}

	public void Grab(Transform grabberTransform)
	{
		transform.parent = grabberTransform;
		transform.localPosition = Vector3.up * 3;
		_rigidBRef.isKinematic = true;
		Grabbed = true;
	}

	public void Eject(Vector3 targetVelocity)
	{
		transform.parent = null;

		_rigidBRef.isKinematic = false;
		_rigidBRef.velocity = targetVelocity;

		Grabbed = false;
	}

	public void Respawn()
	{
		if(ArenaManager.Instance != null)
		{
			IEnumerable<Tile> tilesEnumerator = ArenaManager.Instance.Tiles.Where((Tile t) => t != null).Where((Tile t) => t.Obstacle == null && t.CanFall/* && !t.IsSpawn*/);
			List<Tile> tiles = new List<Tile>(tilesEnumerator);

			if (tiles.Count == 0)
				return;
			
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
