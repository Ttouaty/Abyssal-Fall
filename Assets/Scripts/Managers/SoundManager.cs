using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SoundData
{
	public string Key;
	[FMODUnity.EventRef]
	public string eventString;
}

public class SoundManager : GenericSingleton<SoundManager>
{
	public const string VERSION = "1.1.0";

	[SerializeField]
	private SO_SoundList SoundData;

	private static Dictionary<string, SoundData> _soundDictionnary = new Dictionary<string, SoundData>();

	protected override void Awake()
	{
		base.Awake();

		SoundData[] tempSoundDataArray = SoundData.SoundList.ToArray();
		for (int i = 0; i < tempSoundDataArray.Length; i++)
		{
			if(tempSoundDataArray[i].Key != null && tempSoundDataArray[i] != null)
				_soundDictionnary[tempSoundDataArray[i].Key] = tempSoundDataArray[i];
		}
	}

	private static SoundData GetSound(string soundKey)
	{
		SoundData soundToReturn = _soundDictionnary[soundKey];
		if (soundToReturn == null)
		{
			Debug.LogWarning("Sound: \"" + soundKey + "\" was not found in the SoundManager's soundList.");
			return null;
		}
		return soundToReturn;
	}

	
}
