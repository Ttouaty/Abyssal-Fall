using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : GenericSingleton<SoundManager>
{
	[SerializeField]
	private FmodSoundEvent[] AvailableEvents;

	private Dictionary<string, string> EventDico = new Dictionary<string, string>();

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < AvailableEvents.Length; i++)
		{
			if(AvailableEvents[i].Key.Length != 0 && AvailableEvents[i].FmodEvent.Length != 0)
				EventDico.Add(AvailableEvents[i].Key.ToLower(), AvailableEvents[i].FmodEvent);
		}
	}

	public void PlayOS(string eventKey)
	{
		eventKey = eventKey.ToLower();
		if (EventDico.ContainsKey(eventKey))
			FMODUnity.RuntimeManager.PlayOneShot(EventDico[eventKey]);
		else if (EventDico.ContainsValue(eventKey))
			FMODUnity.RuntimeManager.PlayOneShot(eventKey);
	}

	public void PlayOSAttached(string eventKey, GameObject target)
	{
		eventKey = eventKey.ToLower();
		if (EventDico.ContainsKey(eventKey))
			FMODUnity.RuntimeManager.PlayOneShotAttached(EventDico[eventKey], target);
		else if (EventDico.ContainsValue(eventKey))
			FMODUnity.RuntimeManager.PlayOneShotAttached(eventKey, target);
	}

}


