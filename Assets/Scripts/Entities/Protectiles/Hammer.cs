using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hammer : ABaseProjectile
{
	public Material[] TrailMaterials;

	public ParticleSystem ImpactParticle;
	public ParticleSystem ImpactWallParticle;
	public override void Launch(Vector3 Position, Vector3 Direction, DamageData Shooter, NetworkInstanceId instanceId)
	{
		_maxLifeSpan = 2;
		base.Launch(Position, Direction, Shooter, instanceId);
	}

	protected override void OnLaunch(GameObject launcher)
	{
		base.OnLaunch(launcher);
		GetComponentInChildren<MeshRenderer>().material = launcher.GetComponent<PlayerController>()._characterProp.PropRenderer.material;
		GetComponentInChildren<TrailRenderer>().material = TrailMaterials[launcher.GetComponent<PlayerController>()._playerRef.SkinNumber];
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		Destroy(Instantiate(ImpactParticle.gameObject, transform.position, Quaternion.identity) as GameObject, ImpactParticle.startLifetime + ImpactParticle.duration);
		SoundManager.Instance.PlayOSAttached("Hammer Hit Character", gameObject);
		base.OnHitPlayer(damagedEntity);
	}

	public override void OnHitEnvironnement()
	{
		Destroy(Instantiate(ImpactWallParticle.gameObject, transform.position, Quaternion.identity) as GameObject, ImpactParticle.startLifetime + ImpactParticle.duration);
		CameraManager.Shake(ShakeStrength.Low);
		SoundManager.Instance.PlayOSAttached("Hammer Hit Wall", gameObject);
		base.OnHitEnvironnement();
	}

	//public override void OnHitPlayerClient(GameObject target)
	//{
	//	base.OnHitPlayerClient(target);
	//}

	protected override void OnStop()
	{
		//Destroy(Instantiate(ImpactParticle.gameObject, transform.position, Quaternion.identity) as GameObject, ImpactParticle.startLifetime + ImpactParticle.duration);
		base.OnStop();
	}
}
