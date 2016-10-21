using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class GroundCheck : MonoBehaviour
{
	private PlayerController _playerRef;
	private float _ownSize;

	private RaycastHit _hit;

	private List<int> _colliderIds = new List<int>();

	void Start()
	{
		_ownSize = GetComponent<SphereCollider>().radius;
		_playerRef = GetComponentInParent<PlayerController>();
	}

	void LateUpdate()
	{
		_playerRef.IsGrounded = _colliderIds.Count > 0;

		if (Physics.Raycast(transform.position, Vector3.down, out _hit, _ownSize + 0.1f, 1 << LayerMask.NameToLayer("Ground")))
		{
			if (_hit.transform.gameObject.activeInHierarchy && _hit.transform.GetComponent<Tile>() != null)
				_hit.transform.GetComponent<Tile>().ActivateFall();
		}
	}

	void OnTriggerEnter(Collider colli)
	{
		if (_colliderIds.Count == 0)
			_playerRef.ContactGround();
		_colliderIds.Add(colli.GetInstanceID());
	}

	void OnTriggerExit(Collider colli)
	{
		_colliderIds.Remove(colli.GetInstanceID());
	}
}
