using UnityEngine;
using System.Collections;

public class SoundTrigger : MonoBehaviour
{
	public FmodOneShotSound SoundEvent;
	public bool AutoActivate = false;
	public bool Spatialized = false;
	public bool NetworkSend = false;

	void Start()
	{
		if (AutoActivate)
			Activate();
	}

	public void Activate()
	{
		if(NetworkSend)
		{
			Player.LocalPlayer.CmdPlaySound(SoundEvent.FmodEvent);
			if(Spatialized)
				Debug.LogWarning("SoundTriggers cannot yet be spatialized and NetworkSent.");
		}
		else
		{
			if(Spatialized)
				SoundEvent.Play(gameObject);
			else
				SoundEvent.Play();
		}
	}
}
