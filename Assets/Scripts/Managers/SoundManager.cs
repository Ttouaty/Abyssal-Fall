using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using System.Linq;

public class SoundManager : GenericSingleton<SoundManager>
{
	[SerializeField]
	private FmodSoundEvent[] AvailableEvents;

	private Dictionary<string, string> EventDico = new Dictionary<string, string>();
	private Dictionary<string, EventInstance> InstanceDico = new Dictionary<string, EventInstance>();
	protected override void Awake()
	{
		base.Awake();

		for (int i = 0; i < AvailableEvents.Length; i++)
		{
			if(AvailableEvents[i].Key.Length != 0 && AvailableEvents[i].FmodEvent.Length != 0)
				EventDico.Add(AvailableEvents[i].Key.ToLower(), AvailableEvents[i].FmodEvent);
		}
	}

	public string GetFmodKeyOfOS(string eventKey)
	{
		eventKey = eventKey.ToLower();
		return EventDico.ContainsKey(eventKey) ? EventDico[eventKey] : null;
	}

	public void PlayOS(string eventKey)
	{
		eventKey = eventKey.ToLower();

		if (EventDico.ContainsKey(eventKey))
		{
			if(EventDico[eventKey].Length != 0)
				FMODUnity.RuntimeManager.PlayOneShot(EventDico[eventKey], Camera.main.transform.position + Camera.main.transform.forward);
			else
				Debug.LogWarning("No key given for => "+eventKey);
		}
		else if (EventDico.ContainsValue(eventKey))
			FMODUnity.RuntimeManager.PlayOneShot(eventKey, Camera.main.transform.position + Camera.main.transform.forward);
		else
			Debug.LogError("No event found for key => "+eventKey);
	}

	public void PlayOSAttached(string eventKey, GameObject target)
	{
		eventKey = eventKey.ToLower();
		if (EventDico.ContainsKey(eventKey))
		{
			if (EventDico[eventKey].Length != 0)
				FMODUnity.RuntimeManager.PlayOneShotAttached(EventDico[eventKey], target);
			else
				Debug.LogWarning("No key given for => " + eventKey);
		}
		else if (EventDico.ContainsValue(eventKey))
			FMODUnity.RuntimeManager.PlayOneShotAttached(eventKey, target);
		else
			Debug.LogError("No event found for key => " + eventKey);
	}

	public EventInstance CreateInstance(string eventKey)
	{
		eventKey = eventKey.ToLower();
		if (InstanceDico.ContainsKey(eventKey))
		{
			Debug.Log("instanceDico is not null for new instance with key => "+eventKey+"\nRemoving old instance.");
			PLAYBACK_STATE newPlaybackState;
			InstanceDico[eventKey].getPlaybackState(out newPlaybackState);
			if (newPlaybackState == PLAYBACK_STATE.PLAYING || newPlaybackState == PLAYBACK_STATE.STARTING)
				Debug.LogWarning("Instance was playing and will be stopped.");
			DestroyInstance(eventKey, STOP_MODE.IMMEDIATE);
		}

		if (EventDico.ContainsKey(eventKey))
		{
			if (EventDico[eventKey].Length != 0)
			{
				InstanceDico[eventKey] = FMODUnity.RuntimeManager.CreateInstance(EventDico[eventKey]);
			}
			else
				Debug.LogWarning("No key given for => " + eventKey);
		}
		else if (EventDico.ContainsValue(eventKey))
			InstanceDico[eventKey] = FMODUnity.RuntimeManager.CreateInstance(eventKey);

		return InstanceDico[eventKey];
	}

	public EventInstance GetInstance(string eventKey)
	{
		eventKey = eventKey.ToLower();
		if (InstanceDico.ContainsKey(eventKey))
			return InstanceDico[eventKey];

		return null;
	}

	public void DestroyInstance(string eventKey, STOP_MODE targetStopMode)
	{
		eventKey = eventKey.ToLower();
		if (InstanceDico.ContainsKey(eventKey))
		{
			InstanceDico[eventKey].stop(targetStopMode);
			InstanceDico[eventKey].release();
			InstanceDico.Remove(eventKey);
			Debug.Log("Instance => " + eventKey + " succesfully destroyed.");
		}
		else
		{
			Debug.LogWarning("No instance with key => \""+eventKey+"\" found to destroy");
			foreach (KeyValuePair<string, EventInstance> entry in InstanceDico)
			{
				Debug.Log(entry.Key);
			}
		}
	}

	public void DestroyInstance(EventInstance Event, STOP_MODE targetStopMode)
	{
		if (InstanceDico.ContainsValue(Event))
		{
			string key = InstanceDico.FirstOrDefault(x => x.Value == Event).Key;

			InstanceDico[key].stop(targetStopMode);
			InstanceDico[key].release();
			InstanceDico.Remove(key);
			Debug.Log("Instance => " + key + " succesfully destroyed.");
		}
		else
		{
			foreach (KeyValuePair<string, EventInstance> entry in InstanceDico)
			{
				Debug.Log(entry.Key);
			}
		}
	}

	public void DestroyAllInstances()
	{
		foreach (KeyValuePair<string, EventInstance> sound in InstanceDico)
		{
			sound.Value.stop(STOP_MODE.ALLOWFADEOUT);
			sound.Value.release();

		}

		InstanceDico.Clear();
	}
}


