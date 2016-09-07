using UnityEngine;
using System.Collections;

public class NinjaController : PlayerController {
	[SerializeField]
	private ParticleSystem _ghostParticles;

	private float SpecialLength = 2;

	protected override void SpecialAction()
	{
		TimeCooldown specialCooldown = new TimeCooldown(this);
		specialCooldown.onProgress += OnFantomActive;
		specialCooldown.onFinish += OnFantomFinish;
		specialCooldown.Set(SpecialLength);
		//TODO : ANIM
		_ghostParticles.Clear();
		_ghostParticles.Play();
		gameObject.layer = LayerMask.NameToLayer("PlayerGhost");
	}

	private void OnFantomActive()
	{
		_rigidB.velocity = _rigidB.velocity.ZeroY();
		_activeSpeed = _activeSpeed.ZeroY();
		IsGrounded = true;
	}

	private void OnFantomFinish()
	{
		gameObject.layer = LayerMask.NameToLayer("PlayerDefault");
		_ghostParticles.Stop();
	}

}
