using UnityEngine;
using System.Collections;

public class Hammer : ABaseProjectile {

	public override void Launch(Vector3 Position, Vector3 Direction, DamageData Shooter, int instanceId)
	{
		base.Launch(Position, Direction, Shooter, instanceId);
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
