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
		isSelectingCharacter = false;
		CmdSelectCharacter();
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
					Network.Disconnect();
					Destroy(gameObject);
				}
			}
		}
	}

	[ClientRpc]
	public void RpcOpenTargetSlot(OpenSlots SlotsOpen)
	{
		if (isLocalPlayer && !isSelectingCharacter)
		{
			MenuManager.Instance.OpenCharacterSlot(SlotsOpen, this);

			Debug.LogWarning("player N°"+PlayerNumber+" listening to joystick "+JoystickNumber+" has openned slot "+SlotsOpen.ToString());
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

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log("Clean up after player " + player);
		RpcCloseTargetSlot(PlayerNumber - 1);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}

	[Command]
	public void CmdSelectCharacter()
	{
		isReady = true;
		Debug.Log("PLayer N°"+PlayerNumber+" has selected a character");
	}

}
