using UnityEngine;
using System.Collections;
using System;

public class ParryModule : MonoBehaviour, IDamageable
{
	private PlayerController _playerRef;
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
		GetComponent<Collider>().isTrigger = true;
	}

	public void Damage(Vector3 direction, Vector3 impactPoint, DamageData Sender)
	{
		if (_isParrying)
		{
			_playerRef.Parry(Sender.Projectile);
		}
	}
}
