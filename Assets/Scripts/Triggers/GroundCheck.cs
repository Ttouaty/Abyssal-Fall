using UnityEngine;
using System.Collections;

public class GroundCheck : MonoBehaviour {

	private Rigidbody _rigidB;
	private PlayerController _playerRef;

	private float _checkDistance;
	private RaycastHit _hit;
	
	void Start () 
	{
		_playerRef = GetComponentInParent<PlayerController>();
		_rigidB = GetComponentInParent<Rigidbody>();
		_checkDistance = -(transform.position - _playerRef.transform.position).y;
		CheckForGround();
	}
	
	void FixedUpdate() 
	{
		CheckForGround();
	}

	void CheckForGround()
	{
		if (_playerRef.IsGrounded)
		{
			_rigidB.velocity = _rigidB.velocity.ZeroY();

			if (Physics.Raycast(_playerRef.transform.position, Vector3.down, out _hit, _checkDistance, 1 << LayerMask.NameToLayer("Ground")))
			{
				if (_hit.transform.gameObject.activeInHierarchy)
					_hit.transform.GetComponent<Tile>().ActivateFall();
			}
			else
				_playerRef.IsGrounded = false;
		}
		else if (_rigidB.velocity.y <= 0)
		{
			if (Physics.Raycast(_playerRef.transform.position, Vector3.down, out _hit, _checkDistance + Mathf.Abs(_rigidB.velocity.y) * Time.fixedDeltaTime, 1 << LayerMask.NameToLayer("Ground")))
				_playerRef.ContactGround(); // only activate if you make contact with the ground once
			else
				_playerRef.IsGrounded = false;
		}
	}
}
