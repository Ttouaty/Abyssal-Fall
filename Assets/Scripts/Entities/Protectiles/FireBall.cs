using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class FireBall : ABaseProjectile
{
	private ParticleSystem _movingParticlesRef;

	private float _explosionDelay;
	private float _explosionRadius;
	private Vector3 _ejection;
	private DamageData _explosionDamageData;
	private bool _activated;

	private MageController _launcherRef;

	public void Launch(Vector3 Position, Vector3 Direction, float explosionDelay, float explosionRadius, Vector3 ejection, DamageData newDamageData, NetworkInstanceId instanceId)
	{
		GetComponent<Collider>().enabled = true;
		base.Launch(Position, Direction, newDamageData, instanceId);
		StartCoroutine(DelayStop());
		_explosionRadius = explosionRadius;
		_explosionDelay = explosionDelay;
		_ejection = ejection;
		_explosionDamageData = newDamageData;
		_activated = false;
	}

	private IEnumerator DelayStop()
	{
		yield return new WaitForSeconds(3f);
		Stop();
	}

	public void Activate()
	{
		if (_activated)
			return;

		_rigidB.velocity = Vector3.zero;
		_activated = true;
		RpcExplode();
	}

	protected override void OnLaunch(GameObject Launcher)
	{
		base.OnLaunch(Launcher);

		_launcherRef = Launcher.GetComponent<MageController>();
		SoundManager.Instance.PlayOSAttached("MagicCharge", gameObject);
		_movingParticlesRef = Instantiate(_launcherRef.MoveParticle, transform, false) as ParticleSystem;
		_movingParticlesRef.transform.localPosition = Vector3.zero;
		_movingParticlesRef.Play();
	}

	[ClientRpc]
	public void RpcExplode()
	{
		_rigidB.velocity = Vector3.zero;
		StartCoroutine(DelayedExplosion());
	}

	protected override void Stop()
	{
		if (_movingParticlesRef != null)
			Destroy(_movingParticlesRef);

		base.Stop();
	}

	IEnumerator DelayedExplosion()
	{
		if(_movingParticlesRef != null)
			_movingParticlesRef.Stop();
		
		ParticleSystem preExploParticles = (ParticleSystem)Instantiate(_launcherRef.ImplosionParticle, transform.position, _launcherRef.ImplosionParticle.transform.rotation);
		preExploParticles.Play();
		Destroy(preExploParticles.gameObject, _explosionDelay + preExploParticles.startLifetime);

		yield return new WaitForSeconds(_explosionDelay - preExploParticles.startLifetime * 0.5f);
		preExploParticles.Stop();
		yield return new WaitForSeconds(preExploParticles.startLifetime * 0.5f);

		ParticleSystem exploParticles = (ParticleSystem)Instantiate(_launcherRef.ExplosionParticle, transform.position, _launcherRef.ExplosionParticle.transform.rotation);
		exploParticles.Play();
		Destroy(exploParticles.gameObject, exploParticles.startLifetime + exploParticles.duration);
		SoundManager.Instance.PlayOSAttached("Explosion", exploParticles.gameObject);

		if(NetworkServer.active)
		{
			Collider[] foundElements = Physics.OverlapSphere(transform.position, _explosionRadius);

			for (int i = 0; i < foundElements.Length; i++)
			{
				if (foundElements[i].gameObject.GetComponent<NetworkIdentity>() == null)
					continue;
				if (foundElements[i].gameObject.GetComponent<NetworkIdentity>().netId == LauncherNetId)
					continue;
				if (foundElements[i].GetComponent<IDamageable>() != null)
					foundElements[i].GetComponent<IDamageable>().Damage(
						Quaternion.FromToRotation(Vector3.right, (foundElements[i].transform.position - transform.position).ZeroY().normalized) * _ejection,
						transform.position,
						_explosionDamageData.SetProjectile(this));
			}
		}
	}

	public override void OnHitPlayer(IDamageable damagedEntity)
	{
		//base.OnHitPlayer(damagedEntity);
	}

	public override void OnHitEnvironnement()
	{
		//base.OnHitEnvironnement();
	}
}
