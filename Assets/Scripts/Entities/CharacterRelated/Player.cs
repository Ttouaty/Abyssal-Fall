using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Player : NetworkLobbyPlayer
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

	private bool isSelectingCharacter = false;

	public void Init(int newJoystickNumber)
	{
		if(isLocalPlayer)
			JoystickNumber = newJoystickNumber;
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
		readyToBegin = true;
		CmdSelectCharacter();
	}

	public void UnReady()
	{
		_characterUsed = null;
		isReady = false;
		readyToBegin = false;
	}

	void OnDestroy()
	{
		if (isServer)
		{
			if (Controller != null)
				Destroy(Controller.gameObject);
		}
	}

	public override void OnStartClient()
	{
		//base.OnStartClient();
		//prevents a array index out of range error :x

	}

	public override void OnStartLocalPlayer()
	{
		if (isLocalPlayer)
		{
			if(MenuManager.Instance != null)
			{
				if (MenuManager.Instance.LocalJoystickBuffer.Count != 0)
				{
					Init(MenuManager.Instance.LocalJoystickBuffer[MenuManager.Instance.LocalJoystickBuffer.Count - 1]);
					Debug.Log("player created with joystick number :" + JoystickNumber);
				}
				else
				{
					Debug.Log("No joystickBuffer found in menu manager: closing connection.");
					ServerManager.singleton.StopClient();
					Destroy(gameObject);
				}
			}
		}
	}

	[ClientRpc]
	public void RpcOpenTargetSlot(int slotNumber)
	{
		if (isLocalPlayer && !isSelectingCharacter)
		{
			MenuManager.Instance.OpenCharacterSlot(slotNumber, this);

			Debug.LogWarning("player N°"+PlayerNumber+" listening to joystick "+JoystickNumber+" has openned slot "+slotNumber);
			isSelectingCharacter = true;
		}
	}

	[ClientRpc]
	public void RpcCloseTargetSlot(int slotNumber)
	{
		if (isLocalPlayer)
		{
			MenuManager.Instance.CloseCharacterSlot(slotNumber);
		}
	}

	[Command]
	public void CmdSelectCharacter()
	{
		Debug.Log("PLayer N°"+PlayerNumber+" has selected a character");
	}

}
