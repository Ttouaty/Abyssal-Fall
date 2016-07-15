using UnityEngine;
using System.Collections;

public class Player
{
	public static int NumberOfPlayersAlreadyPresent = -1;
	[HideInInspector]
	public int PlayerNumber;
	[HideInInspector]
	public int JoystickNumber = 0;
	[HideInInspector]
	public int SkinNumber = 0; //the index of the material used by the playerMesh
	public float Score = 0;
	public bool isReady
	{
		get{ return _ready; }
		private set{ _ready = value; }
	}

	private bool _ready;

	private PlayerController _characterUsed;

	public Player()
	{
		PlayerNumber = ++NumberOfPlayersAlreadyPresent;
	}


	public void SelectCharacter(ref PlayerController newCharacter)
	{
		//_characterUsed = newCharacter; //unused for now (to avoid cyclic ref)
		newCharacter._playerRef = this;
	}

	public void ResetScore()
	{
		Score = 0;
	}

	public void Ready(PlayerController linkedCharacter)
	{
		_characterUsed = linkedCharacter;
		isReady = true;
	}

	public void UnReady()
	{
		_characterUsed = null;
		isReady = false;
	}
}
