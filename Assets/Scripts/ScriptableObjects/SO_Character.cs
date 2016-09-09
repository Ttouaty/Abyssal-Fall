using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Abyssal Fall/Character/SO_Character")]
public class SO_Character :  ScriptableObject{
	public string IngameName;
	public Image Icon;
	public CharacterModel CharacterModel;
	public Material[] CharacterMaterials;
	
	[Space()]
	public Stats CharacterStats;
	[Space()]
	public Dash Dash;

	public PlayerAudioList SoundList;

    public PoolConfiguration[] OtherAssetsToLoad;
	//public string[] sounds = { "Not Used for now" };
}
