using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class GroundCheck : MonoBehaviour
{
	private PlayerController _playerRef;
	private Rigidbody _rigidB;
	private bool IsColliding = false;

	void Start()
	{
		_playerRef = GetComponentInParent<PlayerController>();
		_rigidB = GetComponentInParent<Rigidbody>();
	}

	void LateUpdate()
	{
		_playerRef.IsGrounded = IsColliding;
	}

	void OnTriggerEnter(Collider colli)
	{
		if (_rigidB.velocity.y > 0.2f)
			return;

		if (!_playerRef.IsGrounded)
		{
			_playerRef.ContactGround();
		}
		IsColliding = true;
	}
	void OnTriggerStay(Collider colli)
	{
		IsColliding = _rigidB.velocity.y < 0.2f;
	}
	void OnTriggerExit(Collider colli)
	{
		IsColliding = false;
	}
}
