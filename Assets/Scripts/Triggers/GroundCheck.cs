using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

//[RequireComponent(typeof(Collider))]
//public class GroundCheck : MonoBehaviour
//{
//	public static bool noclip = false;

//	private PlayerController _playerRef;
//	private Collider _colliderRef;
//	private Rigidbody _rigidBRef;

//	[HideInInspector]
//	public bool HasCheckedForColli = false;
//	//private float _ownSize;

//	//private RaycastHit _hit;

//	private List<int> _colliderIds = new List<int>();

//	void Start()
//	{
//		//_ownSize = GetComponent<SphereCollider>().radius;
//		_playerRef = GetComponentInParent<PlayerController>();
//		_colliderRef = GetComponent<Collider>();
//		_rigidBRef = _playerRef.GetComponent<Rigidbody>();
//		gameObject.layer = LayerMask.NameToLayer("GroundCheck");
//	}

//	void Update()
//	{
//		//if (_playerRef._playerRef != null)
//		//	if (!_playerRef._isLocalPlayer)
//		//		enabled = false; // Deactivate if character is not local

//		if (_rigidBRef.velocity.y > 1f)
//		{
//			if (_colliderRef.enabled)
//			{
//				_colliderRef.enabled = false;
//				_colliderIds.Clear();
//			}
//		}
//		else
//		{
//			_colliderRef.enabled = true;
//		}
//	}

//	void LateUpdate()
//	{
//		if (_playerRef._forceGroundedTimer == null)
//			return;

//		_playerRef.IsGrounded = _colliderIds.Count > 0 || _playerRef._forceGroundedTimer.TimeLeft != 0;
//		if(noclip)
//			_playerRef.IsGrounded = true;
//	}

//	void FixedUpdate()
//	{
//		HasCheckedForColli = true;
//	}

//	void OnTriggerEnter(Collider colli)
//	{
//		if (!enabled)
//			return;

//		if (_colliderIds.Count == 0 && _playerRef != null)
//			_playerRef.ContactGround();

//		if(!_colliderIds.Contains(colli.GetInstanceID()))
//			_colliderIds.Add(colli.GetInstanceID());

//		if (colli.gameObject.activeInHierarchy && colli.GetComponent<Tile>() != null && !_playerRef.IsDead && _playerRef._isLocalPlayer && _playerRef.IsInitiated)
//			colli.GetComponent<Tile>().ActivateFall();
//	}

//	void OnTriggerExit(Collider colli)
//	{
//		if (!enabled)
//			return;
//		_colliderIds.Remove(colli.GetInstanceID());
//	}

//	public void Activate()
//	{
//		enabled = true;
//		_colliderRef = GetComponent<Collider>();
//		_colliderRef.enabled = false; // force switch between true & false
//		_colliderRef.enabled = true;
//		HasCheckedForColli = false;
//	}

//	public void Deactivate()
//	{
//		enabled = false;
//		_colliderIds.Clear();

//		_colliderRef = GetComponent<Collider>();
//		_colliderRef.enabled = false;
//	}
//}

public class GroundCheck : MonoBehaviour
{
	public static bool noclip = false;

	public float MaxSlopeAngle = 45f;
	public Collider GroundCheckCollider;
	[HideInInspector]
	public bool IsGrounded = false;
	[HideInInspector]
	public Vector3 ContactNormal = new Vector3(0, 1, 0);
	private Vector3 _contactNormalTemp = new Vector3(0, 1, 0);

	public delegate void OnTileCollision(Tile t);
	public OnTileCollision OnTileContact;


	private bool _isTouching;

	void Awake()
	{
		enabled = false;
	}

	void FixedUpdate()
	{
		ContactNormal = _contactNormalTemp;
		IsGrounded = _isTouching;
		_isTouching = false;
		_contactNormalTemp.Set(0, 1, 0);
		if(noclip)
			IsGrounded = true;
	}

	void OnCollisionEnter(Collision colli)
	{
		if (!enabled)
			return;
		if(OnTileContact != null)
			OnTileContact(colli.gameObject.GetComponent<Tile>());
	}

	void OnCollisionStay(Collision colli)
	{
		if (!enabled)
			return;

		_contactNormalTemp.Set(0, 0, 0);

		for (int i = 0; i < colli.contacts.Length; i++)
		{
			if (colli.contacts[i].thisCollider != GroundCheckCollider)
				continue;

			if (Vector3.Angle(colli.contacts[i].normal, Vector3.up) < MaxSlopeAngle)
			{
				_isTouching = true;
				_contactNormalTemp += colli.contacts[i].normal;
				//Debug.DrawLine(colli.contacts[i].point, colli.contacts[i].point + colli.contacts[i].normal, Color.red, 0,false);
			}
		}

		if (_contactNormalTemp.magnitude == 0)
			_contactNormalTemp.y = 1;
		else
			_contactNormalTemp.Normalize();
	}

	public void Activate()
	{
		enabled = true;
	}

	public void Deactivate()
	{
		enabled = false;
	}

	
}
