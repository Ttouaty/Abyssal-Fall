using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New_SO_SoundList", menuName = "Abyssal Fall/Sound/New Sound List")]
public class SO_SoundList : ScriptableObject
{
	public List<SoundData> SoundList = new List<SoundData>();
}
