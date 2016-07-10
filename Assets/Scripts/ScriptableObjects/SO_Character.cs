using UnityEngine;
using System.Collections;
using UnityEditor;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character/SO_Character")]
public class SO_Character :  ScriptableObject{
	public string IngameName;
	public CharacterModel CharacterModel;
	public Material[] CharacterMaterials;
	
	[Space()]
	public Stats CharacterStats;
	[Space()]
	public Dash Dash;

	public PlayerAudioList SoundList;
	//public string[] sounds = { "Not Used for now" };
}
