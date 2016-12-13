using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

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
		if (Controller != null)
			Destroy(Controller.gameObject);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		//prevents a array index out of range error :x

	}

	public override void OnStartLocalPlayer()
	{
		if (isLocalPlayer)
		{
			name += "_" + PlayerNumber;

			if (MenuManager.Instance != null)
			{
				if (MenuManager.Instance.LocalJoystickBuffer.Count != 0)
				{
					Init(MenuManager.Instance.LocalJoystickBuffer[MenuManager.Instance.LocalJoystickBuffer.Count - 1]);
					Debug.Log("player"+name+" created with joystick number :" + JoystickNumber);
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

	[Command]
	public void CmdRequestOpenSlot(int newNumber)
	{
	}

	[ClientRpc]
	public void RpcOpenSlot(string slotsToOpen, int playerOpenning)
	{

		Debug.Log("Object with netId "+ netId+" has received RpcOpenSLot() call ");
		if (isLocalPlayer)
		{
			OpenSlots NewSlotToOpen = (OpenSlots)Enum.Parse(typeof(OpenSlots), slotsToOpen);
			int i = -1;
			foreach (OpenSlots slot in Enum.GetValues(typeof(OpenSlots)))
			{
				if ((NewSlotToOpen & slot) != 0 && i != -1)
				{
					MenuManager.Instance.OpenCharacterSlot(slot, playerOpenning == (i+1) ? this : null); //open his own slot
					Debug.Log("openning slots: "+ slot + " and is target player: "+(playerOpenning == (i + 1)));
				}
				i++;
			}

			//Debug.LogWarning("player N°"+PlayerNumber+" listening to joystick "+JoystickNumber+" has openned slot "+ NewSlotToOpen.ToString());
			isSelectingCharacter = true;
		}
		else
		{
			Debug.Log("is non localPlayer "+name);
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

	[ClientRpc]
	public void RpcPingClient(string message)
	{
		Debug.LogError("Client with id "+netId+" received the ping: "+message);
	}

}
