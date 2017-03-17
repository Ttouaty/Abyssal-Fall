using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Shuriken : ABaseProjectile
{
	public ParticleSystem _smokeTrail;

	protected override void Awake()
	{
		base.Awake();
		_maxLifeSpan = 0.5f;
	}

	public override void Launch(Vector3 Position, Vector3 Direction, DamageData data, NetworkInstanceId newLauncherId)
	{
		_smokeTrail.transform.localPosition = Vector3.zero;
		_smokeTrail.Play();
		base.Launch(Position, Direction, data, newLauncherId);
	}

	protected override void Stop()
	{
		ParticleSystem OldSmokeTrail = _smokeTrail;
		_smokeTrail = Instantiate(_smokeTrail, transform, false) as ParticleSystem;
		_smokeTrail.transform.localPosition = Vector3.zero;
		OldSmokeTrail.transform.parent = null;
		OldSmokeTrail.Stop();
		GameObject.Destroy(OldSmokeTrail.gameObject, OldSmokeTrail.startLifetime);
		base.Stop();
	}

	protected override void OnLaunch(GameObject launcher)
	{
		base.OnLaunch(launcher);
		GetComponentInChildren<MeshRenderer>().material = launcher.GetComponent<PlayerController>()._characterProp.PropRenderer.material;
	}
}
