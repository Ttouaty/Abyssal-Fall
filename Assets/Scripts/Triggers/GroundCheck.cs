using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class GroundCheck : MonoBehaviour
{
	private PlayerController _playerRef;
	private Collider _colliderRef;
	private Rigidbody _rigidBRef;
	//private float _ownSize;

	//private RaycastHit _hit;

	private List<int> _colliderIds = new List<int>();

	void Start()
	{
		//_ownSize = GetComponent<SphereCollider>().radius;
		_playerRef = GetComponentInParent<PlayerController>();
		_colliderRef = GetComponent<Collider>();
		_rigidBRef = _playerRef.GetComponent<Rigidbody>();

		if(!_playerRef._playerRef.isLocalPlayer)
			enabled = false; // Deactivate if character is not local
	}

	void Update()
	{
		if (_rigidBRef.velocity.y > 1f)
		{
			if (_colliderRef.enabled)
			{
				_colliderRef.enabled = false;
				_colliderIds.Clear();
			}
		}
		else
		{
			_colliderRef.enabled = true;
		}
	}

	void LateUpdate()
	{
		_playerRef.IsGrounded = _colliderIds.Count > 0;
	}

	void OnTriggerEnter(Collider colli)
	{
		if (!enabled)
			return;

		if (_colliderIds.Count == 0)
			_playerRef.ContactGround();
		_colliderIds.Add(colli.GetInstanceID());

		if (colli.gameObject.activeInHierarchy && colli.GetComponent<Tile>() != null)
			colli.GetComponent<Tile>().ActivateFall();
	}

	void OnTriggerExit(Collider colli)
	{
		if (!enabled)
			return;
		_colliderIds.Remove(colli.GetInstanceID());
	}
}
