using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class GroundCheck : MonoBehaviour
{
	private PlayerController _playerRef;
	private Rigidbody _rigidB;
	private bool IsColliding = false;

	private RaycastHit _hit;

	void Start()
	{
		_playerRef = GetComponentInParent<PlayerController>();
		_rigidB = GetComponentInParent<Rigidbody>();
	}

	void LateUpdate()
	{
		_playerRef.IsGrounded = IsColliding;

		if (Physics.Raycast(_playerRef.transform.position, Vector3.down, out _hit, 1.2f, 1 << LayerMask.NameToLayer("Ground")))
		{
			if (_hit.transform.gameObject.activeInHierarchy && _hit.transform.GetComponent<Tile>() != null)
				_hit.transform.GetComponent<Tile>().ActivateFall();
		}
	}

	void OnTriggerEnter(Collider colli)
	{
		if (_rigidB.velocity.y > 0.1f)
			return;

		if (!_playerRef.IsGrounded)
		{
			_playerRef.ContactGround();
		}
		IsColliding = true;
	}
	void OnTriggerStay(Collider colli)
	{
		IsColliding = _rigidB.velocity.y < 0.1f;
	}
	void OnTriggerExit(Collider colli)
	{
		IsColliding = false;
	}
}
/*
           


*/
