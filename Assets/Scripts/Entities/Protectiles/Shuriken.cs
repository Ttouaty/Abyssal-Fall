using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Shuriken : ABaseProjectile
{
	public GameObject[] _smokeTrails;
	private int activeSkinNumber = 0;
	protected override void Awake()
	{
		base.Awake();
		_maxLifeSpan = 0.6f;
	}

	public override void Launch(Vector3 Position, Vector3 Direction, DamageData data, NetworkInstanceId newLauncherId)
	{
		base.Launch(Position, Direction, data, newLauncherId);
	}

	protected override void OnStop()
	{
		GameObject OldSmokeTrail = GetComponentInChildren<TrailRenderer>(false).gameObject;
		_smokeTrails[activeSkinNumber] = Instantiate(_smokeTrails[activeSkinNumber], transform, false) as GameObject;
		_smokeTrails[activeSkinNumber].transform.localPosition = Vector3.zero;
		OldSmokeTrail.transform.parent = null;
		OldSmokeTrail.GetComponentInChildren<ParticleSystem>().Stop();
		Destroy(OldSmokeTrail.gameObject, OldSmokeTrail.GetComponentInChildren<ParticleSystem>().startLifetime);
		base.OnStop();
	}

	protected override void Stop()
	{
		base.Stop();
	}

	public override void OnHitEnvironnement()
	{
		SoundManager.Instance.PlayOSAttached("Shuriken Hit Wall", gameObject);
		base.OnHitEnvironnement();
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		SoundManager.Instance.PlayOSAttached("Shuriken Hit Character", gameObject);
		base.OnHitPlayer(damagedEntity);
	}

	protected override void OnLaunch(GameObject launcher)
	{
		base.OnLaunch(launcher);
		activeSkinNumber = launcher.GetComponent<PlayerController>()._playerRef.SkinNumber;

		for (int i = 0; i < _smokeTrails.Length; i++)
		{
			_smokeTrails[i].SetActive(i == activeSkinNumber);
		}

		_smokeTrails[activeSkinNumber].transform.localPosition = Vector3.zero;
		_smokeTrails[activeSkinNumber].GetComponentInChildren<ParticleSystem>().Play();
		GetComponentInChildren<MeshRenderer>().material = launcher.GetComponent<PlayerController>()._characterProp.PropRenderer.material;
	}
}
