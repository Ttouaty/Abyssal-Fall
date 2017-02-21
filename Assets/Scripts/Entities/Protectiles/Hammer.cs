using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Hammer : ABaseProjectile
{
	public override void Launch(Vector3 Position, Vector3 Direction, DamageData Shooter, int instanceId)
	{
		base.Launch(Position, Direction, Shooter, instanceId);
	}

	protected override void OnLaunch(GameObject launcher)
	{
		GetComponentInChildren<MeshRenderer>().material = launcher.GetComponent<PlayerController>()._characterProp.PropRenderer.material;
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		base.OnHitPlayer(damagedEntity);
	}

	public override void OnHitEnvironnement()
	{
		//base.OnHitEnvironnement();
	}
}
