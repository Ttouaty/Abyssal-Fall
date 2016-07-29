using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct SoundData
{
	public string Name;
	public AudioClip Clip;
}

[System.Serializable]
public struct ChanelData
{
	public string Name;
	public bool Loop;
}

public class SoundManager : GenericSingleton<SoundManager>
{
	public const string VERSION = "1.0.0";
	public const string DEFAULT_CHANEL_NAME = "global";

	public List<ChanelData> AudioSourceChanels = new List<ChanelData>();
	public List<SoundData> SoundList = new List<SoundData>();

	private Dictionary<string, AudioSource> _audioSourceChanels;
	private Dictionary<string, AudioClip> SoundMap;


    void Awake()
	{
		// Générer un canal audio par valeur dans l'enum EAudioSourcePistes
		_audioSourceChanels = new Dictionary<string, AudioSource>();
		_audioSourceChanels.Add(DEFAULT_CHANEL_NAME, gameObject.AddComponent<AudioSource>()); // Add Default chanel
		for (int i = 0; i < AudioSourceChanels.Count; ++i)
		{
			AudioSource chanel = gameObject.AddComponent<AudioSource>();
			chanel.loop = AudioSourceChanels[i].Loop;
			_audioSourceChanels.Add(AudioSourceChanels[i].Name, chanel);
		}

		// Générer une map de <NomDuSon, AudioClip>
		SoundMap = new Dictionary<string, AudioClip>();
		for (int i = 0; i < SoundList.Count; ++i)
		{
			SoundMap.Add(SoundList[i].Name, SoundList[i].Clip);
		}
	}

	public void AddSoundClip (AudioClip sound)
	{
		SoundData data = new SoundData();
		data.Name = sound.name;
		data.Clip = sound;

		if (!SoundList.Contains(data))
		{
			SoundList.Add(data);
		}
	}

	public void AddChanel ()
	{
		ChanelData data = new ChanelData();
		data.Name = "";
		AudioSourceChanels.Add(data);
	}

	public void RemoveSoundClip(SoundData data)
	{
		SoundList.Remove(data);
	}

	public void RemoveChanel(ChanelData data)
	{
		AudioSourceChanels.Remove(data);
	}

	public bool PlaySound (string soundName, string chanel = null)
	{
		if (SoundMap.ContainsKey(soundName))
		{
			AudioSource audioSourceChanel;
			if(!_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
			{
				audioSourceChanel = _audioSourceChanels[DEFAULT_CHANEL_NAME];
			}
			audioSourceChanel.clip = SoundMap[soundName];
			audioSourceChanel.Play();
			return true;
		}
		return false;
	}

	public bool PauseSound(string soundName, string chanel = null)
	{
		if (SoundMap.ContainsKey(soundName))
		{
			AudioSource audioSourceChanel;
			if (_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
			{
				if (audioSourceChanel.clip.name == soundName)
				{
					audioSourceChanel.Pause();
				}
			}
			return true;
		}
		return false;
	}

	public bool StopSound(string soundName, string chanel = null)
	{
		if (SoundMap.ContainsKey(soundName))
		{
			AudioSource audioSourceChanel;
			if (_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
			{
				if (audioSourceChanel.clip.name == soundName)
				{
					audioSourceChanel.Stop();
				}
			}
			return true;
		}
		return false;
	}

	public bool PlayChanel(string chanel)
	{
		AudioSource audioSourceChanel;
		if (_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
		{
			audioSourceChanel.Play();
			return true;
		}
		return false;
	}

	public bool PauseChanel(string chanel)
	{
		AudioSource audioSourceChanel;
		if (_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
		{
			audioSourceChanel.Pause();
			return true;
		}
		return false;
	}

	public bool StopChanel(string chanel)
	{
		AudioSource audioSourceChanel;
		if (_audioSourceChanels.TryGetValue(chanel, out audioSourceChanel))
		{
			audioSourceChanel.Stop();
			return true;
		}
		return false;
	}
}
