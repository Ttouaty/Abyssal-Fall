using UnityEngine;
using System.Collections;

public class SoundTrigger : MonoBehaviour
{
	public FmodOneShotSound SoundEvent;
	public bool AutoActivate = false;
	public bool Spatialized = false;

	void Start()
	{
		if (AutoActivate)
			Activate();
	}

	public void Activate()
	{
		if(Spatialized)
			SoundEvent.Play(gameObject);
		else
			SoundEvent.Play(Camera.main.gameObject);
	}
}
