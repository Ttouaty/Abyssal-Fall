using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Abyssal Fall/Character/SO_Character")]
public class SO_Character :  ScriptableObject{
	public string IngameName;
	public Image Icon;
	public CharacterModel CharacterModel;
	public Material[] CharacterMaterials;
	
	[Space()]
	public Stats CharacterStats;
	[HideInInspector]
	public Vector3 SpecialEjection = new Vector3(4.5f,3.27f); // base ejection is 3,3.27f (* 1.5f because tile scale) (Vector3 used for extensions)
	[Space()]
	public Dash Dash;

	public PlayerAudioList SoundList;

	public PoolConfiguration[] OtherAssetsToLoad;
	//public string[] sounds = { "Not Used for now" };
}
