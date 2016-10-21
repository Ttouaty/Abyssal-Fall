using UnityEngine;
using System.Collections;

public class TileAutoRepop : MonoBehaviour
{
	private Tile _tileRef;

	public float DelayBeforeReactivation = 3;
	private float _timeLeft = 0;

	private void Start()
	{
		_tileRef = GetComponent<Tile>();

		if (_tileRef == null)
			Debug.LogError("NO Tile component found for TileAutoRepop Script: " + gameObject.name);

		_timeLeft = DelayBeforeReactivation;
	}

	private void Update()
	{
		if (!_tileRef.RigidBRef.isKinematic)
		{
			if (_timeLeft <= 0)
			{
				RepopTile();
				_timeLeft = DelayBeforeReactivation;
			}
			_timeLeft -= TimeManager.DeltaTime;
		}
	}

	private void RepopTile()
	{
		_tileRef.ActivateRespawn();
	}

}
