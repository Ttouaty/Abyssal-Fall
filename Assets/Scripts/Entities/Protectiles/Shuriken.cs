using UnityEngine;
using System.Collections;

public class Shuriken : ABaseProjectile
{
	public ParticleSystem _smokeTrail;

	protected override void Awake()
	{
		base.Awake();
		_maxLifeSpan = 0.5f;
	}

	public override void Launch(Vector3 Position, Vector3 Direction, DamageData data, int newLauncherId)
	{
		_smokeTrail.Play();
		base.Launch(Position, Direction, data, newLauncherId);
	}

	protected override void Stop()
	{
		ParticleSystem OldSmokeTrail = _smokeTrail;
		_smokeTrail = Instantiate(_smokeTrail) as ParticleSystem;
		Destroy(OldSmokeTrail.gameObject, OldSmokeTrail.startLifetime);
		OldSmokeTrail.transform.parent = null;
		base.Stop();
	}
}
