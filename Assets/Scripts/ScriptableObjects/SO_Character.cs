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
	public Vector3 SpecialEjection = new Vector3(2.25f,4.905f); // base ejection is 1.5f,4.905f (* 1.5f because tile scale) (Vector3 used for extensions) this vector is equal to 1m ejection with x2 gravity
	[Space()]
	public Dash Dash;

	[Space]
	public DamageData DashDamageData = new DamageData();
	public DamageData SpecialDamageData = new DamageData();


	public PlayerAudioList SoundList;

	public PoolConfiguration[] OtherAssetsToLoad;
	//public string[] sounds = { "Not Used for now" };
}
