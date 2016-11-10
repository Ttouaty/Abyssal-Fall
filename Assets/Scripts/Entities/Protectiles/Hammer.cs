using UnityEngine;
using System.Collections;

public class Hammer : ABaseProjectile {

	public override void Launch(Vector3 Position, Vector3 Direction, DamageDealer Shooter)
	{
		base.Launch(Position, Direction, Shooter);
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
