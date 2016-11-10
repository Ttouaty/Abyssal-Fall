using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Abyssal Fall/Character/SO_Character")]
public class SO_Character :  ScriptableObject{
	public static  Vector3 SpecialEjection = new Vector3(2.25f,4.905f); // base ejection is 1.5f,4.905f (* 1.5f because tile scale) (Vector3 used for extensions) this vector is equal to 1m ejection with x2 gravity
	public string IngameName;
	public Image Icon;
	public CharacterModel CharacterModel;
	public Material[] CharacterMaterials;
	
	[Header("Stats")]
	public Stats CharacterStats;
	[Header("Dash")]
	public Dash Dash;
	public DamageData DashDamageData = new DamageData();

	[Header("Special")]
	public DamageData SpecialDamageData = new DamageData();
	public float SpecialCoolDown = 3;
	public float SpecialLag = 0.2f;

	[Header("Sounds")]
	public PlayerSoundList SoundList;
	[Space]
	[Header("Pool")]
	public PoolConfiguration[] OtherAssetsToLoad;
	//public string[] sounds = { "Not Used for now" };
}
