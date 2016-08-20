using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Abyssal Fall/Character Configuration")]
public class SO_Character :  ScriptableObject
{
	public string IngameName;
    public CharacterModel CharacterModel;
	public Material[] CharacterMaterials;
	
	[Space()]
	public Stats CharacterStats;
	[Space()]
	public Dash Dash;

	public PlayerAudioList SoundList;

    public PoolConfiguration[] OtherAssetsToLoad;
}
