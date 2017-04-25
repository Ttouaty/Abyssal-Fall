using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[RequireComponent(typeof(SphereCollider))]
public class GroundCheck : MonoBehaviour
{
	public static bool noclip = false;

	private PlayerController _playerRef;
	private SphereCollider _colliderRef;
	private Rigidbody _rigidBRef;

	[HideInInspector]
	public bool HasCheckedForColli = false;
	//private float _ownSize;

	//private RaycastHit _hit;

	private List<int> _colliderIds = new List<int>();

	void Start()
	{
		//_ownSize = GetComponent<SphereCollider>().radius;
		_playerRef = GetComponentInParent<PlayerController>();
		_colliderRef = GetComponent<SphereCollider>();
		_rigidBRef = _playerRef.GetComponent<Rigidbody>();
		gameObject.layer = LayerMask.NameToLayer("GroundCheck");
	}

	void Update()
	{
		//if (_playerRef._playerRef != null)
		//	if (!_playerRef._isLocalPlayer)
		//		enabled = false; // Deactivate if character is not local

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
		if(noclip)
			_playerRef.IsGrounded = true;
	}

	void FixedUpdate()
	{
		HasCheckedForColli = true;
	}

	void OnTriggerEnter(Collider colli)
	{
		if (!enabled)
			return;

		if (_colliderIds.Count == 0 && _playerRef != null)
			_playerRef.ContactGround();

		if(!_colliderIds.Contains(colli.GetInstanceID()))
			_colliderIds.Add(colli.GetInstanceID());
	
		if (colli.gameObject.activeInHierarchy && colli.GetComponent<Tile>() != null && !_playerRef.IsDead && _playerRef._isLocalPlayer)
			colli.GetComponent<Tile>().ActivateFall();
	}

	void OnTriggerExit(Collider colli)
	{
		if (!enabled)
			return;
		_colliderIds.Remove(colli.GetInstanceID());
	}

	public void Activate()
	{
		enabled = true;
		_colliderRef = GetComponent<SphereCollider>();
		_colliderRef.enabled = false; // force switch between true & false
		_colliderRef.enabled = true;
		HasCheckedForColli = false;
	}

	public void Deactivate()
	{
		enabled = false;
		_colliderIds.Clear();

		_colliderRef = GetComponent<SphereCollider>();
		_colliderRef.enabled = false;
	}
}
