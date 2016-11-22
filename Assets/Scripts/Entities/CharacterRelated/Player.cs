using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkBehaviour
{

	[HideInInspector]
	public int JoystickNumber = 0;
	[HideInInspector]
	
	public int Score = 0;
	public bool isReady
	{
		get{ return _ready; }
		private set{ _ready = value; }
	}

	[SyncVar]
	private bool _ready;
	[HideInInspector]
	[SyncVar]
	public int SkinNumber = 0; //the index of the material used by the playerMesh
	[HideInInspector]
	[SyncVar]
	public int PlayerNumber;
	[HideInInspector]
	public PlayerController Controller;// PlayerController Instantiated

	// PlayerController Prefab Model
	private PlayerController _characterUsed;
	public PlayerController CharacterUsed
	{
		get
		{
			return _characterUsed;
		}
	}

	void Awake()
	{
		PlayerNumber = ServerManager.Instance.RegisteredPlayers.Count + 1;
		if (MenuManager.Instance != null)
		{
			for (int i = 0; i < MenuManager.Instance.LocalJoystickBuffer.Count; i++)
			{
				JoystickNumber = MenuManager.Instance.LocalJoystickBuffer[i];
				MenuManager.Instance.LocalJoystickBuffer.RemoveAt(i);
				break;
			}
		}
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

	public void Ready(PlayerController linkedCharacter, int indexSkinUsed)
	{
		_characterUsed = linkedCharacter;
		SkinNumber = indexSkinUsed;
		isReady = true;
	}

	public void UnReady()
	{
		_characterUsed = null;
		isReady = false;
	}

	void OnDestroy()
	{
		if (isServer)
		{
			if (Controller != null)
				Destroy(Controller.gameObject);
		}
	}
}
