using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class AnimationToolkit : MonoBehaviour
{
	private Dictionary<string, ParticleSystem> _availableParticleSystems = new Dictionary<string, ParticleSystem>();

	void Awake()
	{
		InitParticles();
	}

	public void InitParticles()
	{
		ParticleSystem[] tempParticles = GetComponentsInChildren<ParticleSystem>(false);

		_availableParticleSystems.Clear();

		for (int i = 0; i < tempParticles.Length; i++)
		{
			if (tempParticles[i].transform.parent.GetComponent<ParticleSystem>() == null)
				_availableParticleSystems[tempParticles[i].name.ToLower()] = tempParticles[i];
		}
	}

	public ParticleSystem GetParticleSystem(string particleName)
	{
		particleName = particleName.ToLower();
		if (_availableParticleSystems.ContainsKey(particleName))
			return _availableParticleSystems[particleName];
		else
		{
			Debug.LogError("No particle with name => " + particleName + " in Go => " + gameObject.name);
			Debug.Break();
		}
		return null;
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

	public void EndVictory()
	{
		ArenaManager.Instance.victoryAnimationIsFinished = true;
	}
}
