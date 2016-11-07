using UnityEngine;
using System.Collections;

public class Hammer : ABaseProjectile {

	public AudioClip OnHitPlayerSound;
	public AudioClip OnHitObstacle;

	public override void Launch(Vector3 Position, Vector3 Direction, DamageDealer Shooter)
	{
		base.Launch(Position, Direction, Shooter);
		StartCoroutine(DelayStop());
	}

	private IEnumerator DelayStop()
	{
		yield return new WaitForSeconds(3f);
		Stop();
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		base.OnHitPlayer(damagedEntity);
		_audioSource.PlayOneShot(OnHitPlayerSound);
	}

	public override void OnHitEnvironnement()
	{
		//base.OnHitEnvironnement();
	}
}
