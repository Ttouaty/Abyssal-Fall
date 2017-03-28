using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationToolkit : MonoBehaviour
{
	private Dictionary<string, ParticleSystem> _availableParticleSystems = new Dictionary<string, ParticleSystem>();

	void Awake()
	{
		ParticleSystem[] tempParticles = GetComponentsInChildren<ParticleSystem>();

		_availableParticleSystems.Clear();

		for (int i = 0; i < tempParticles.Length; i++)
		{
			if(tempParticles[i].transform.parent.GetComponent<ParticleSystem>() == null)
				_availableParticleSystems[tempParticles[i].name.ToLower()] = tempParticles[i];
		}
	}

	public void PlaySound(string targetSoundKey)
	{
		Debug.Log("need to make sound => "+targetSoundKey);
	}

	public void CameraShake(ShakeStrength force)
	{
		CameraManager.Shake(force);
	}

	public void ActivateParticle(string particleSystemName)
	{
		particleSystemName = particleSystemName.ToLower();

		if (_availableParticleSystems.ContainsKey(particleSystemName))
			_availableParticleSystems[particleSystemName].Play();
		else
			Debug.LogError("ParticleSystemName => \""+ particleSystemName+"\" was not found in object => "+gameObject.name);
	}
}
