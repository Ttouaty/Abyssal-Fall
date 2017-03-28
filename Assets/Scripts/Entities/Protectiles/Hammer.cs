using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hammer : ABaseProjectile
{
	public ParticleSystem ImpactParticle;
	public override void Launch(Vector3 Position, Vector3 Direction, DamageData Shooter, NetworkInstanceId instanceId)
	{
		base.Launch(Position, Direction, Shooter, instanceId);
	}

	protected override void OnLaunch(GameObject launcher)
	{
		base.OnLaunch(launcher);
		GetComponentInChildren<MeshRenderer>().material = launcher.GetComponent<PlayerController>()._characterProp.PropRenderer.material;

		GetComponentInChildren<TrailRenderer>().material = launcher.GetComponentInChildren<CharacterModel>().MaterialsInUse[2];
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		Destroy(Instantiate(ImpactParticle.gameObject, transform.position, Quaternion.identity) as GameObject, ImpactParticle.startLifetime + ImpactParticle.duration);
		base.OnHitPlayer(damagedEntity);
	}

	public override void OnHitEnvironnement()
	{
		base.OnHitEnvironnement();
	}

	//public override void OnHitPlayerClient(GameObject target)
	//{
	//	base.OnHitPlayerClient(target);
	//}

	protected override void OnStop()
	{
		Destroy(Instantiate(ImpactParticle.gameObject, transform.position, Quaternion.identity) as GameObject, ImpactParticle.startLifetime + ImpactParticle.duration);
		base.OnStop();
	}
}
