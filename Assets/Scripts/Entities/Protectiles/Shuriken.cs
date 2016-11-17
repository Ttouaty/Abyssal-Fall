using UnityEngine;
using System.Collections;

public class Shuriken : ABaseProjectile
{
	public ParticleSystem _smokeTrail;
	private ParticleSystem _smokeTrailInstance;

	protected override void Awake()
	{
		base.Awake();
		_maxLifeSpan = 0.5f;
	}

	public override void Launch(Vector3 Position, Vector3 Direction, DamageData data, int newLauncherId)
	{
		_smokeTrailInstance = Instantiate(_smokeTrail, transform.position, Quaternion.identity, transform) as ParticleSystem;
		base.Launch(Position, Direction, data, newLauncherId);
	}

	protected override void Stop()
	{
		_smokeTrailInstance.Stop();
		Destroy(_smokeTrailInstance.gameObject, _smokeTrailInstance.startLifetime);
		_smokeTrailInstance.transform.parent = null;
		base.Stop();
	}
}
