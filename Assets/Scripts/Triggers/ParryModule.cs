using UnityEngine;
using System.Collections;
using System;

public class ParryModule : MonoBehaviour, IDamageable
{
	private PlayerController _playerRef;
	private Collider _colliRef;

	private bool _isParrying
	{
		get
		{
			return _playerRef._isParrying;
		}
	}

	void Start()
	{
		_playerRef = GetComponentInParent<PlayerController>();
		_colliRef = GetComponent<Collider>();
		_colliRef.isTrigger = true;
	}

	void Update()
	{
		if(_playerRef._playerRef != null)
			_colliRef.enabled = _isParrying;
	}

	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData Sender)
	{
		if (_isParrying)
		{
			_playerRef.Parry(Sender.Projectile);
		}
	}

	public int GetTeamIndex()
	{
		return _playerRef.GetTeamIndex();
	}
}
