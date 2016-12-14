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
					Debug.Log("player "+name+" created with joystick number: " + JoystickNumber);
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
	public void RpcOpenExistingSlots(string slotsToOpen)
	{
		if (isLocalPlayer)
		{
			OpenSlots NewSlotToOpen = (OpenSlots)Enum.Parse(typeof(OpenSlots), slotsToOpen);
			int i = 0;
			foreach (OpenSlots slot in Enum.GetValues(typeof(OpenSlots)))
			{
				if ((NewSlotToOpen & slot) != 0 && i != 0)
				{
					Debug.Log("openning slots: " + slot + " with player == null");
					MenuManager.Instance.OpenCharacterSlot(slot, null); //open his own slot
				}
				i++;
			}
		}
	}

	[ClientRpc]
	public void RpcOpenSlot(string slotToOpen, GameObject OwnerPlayer, int newPlayerNumber)
	{
		if (isLocalPlayer)
		{
			if (PlayerNumber == 0)
				PlayerNumber = newPlayerNumber;
			OpenSlots NewSlotToOpen = (OpenSlots)Enum.Parse(typeof(OpenSlots), slotToOpen);
			MenuManager.Instance.OpenCharacterSlot(NewSlotToOpen, OwnerPlayer.GetComponent<Player>()); //open his own slot
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

	//[Command]
	//public void CmdCloseTargetSlot(int slotNumber)
	//{
	//	Debug.LogError("close slot");
	//	if (!ServerManager.Instance.IsInLobby)
	//		return;

	//	for (int i = 0; i < ServerManager.Instance.RegisteredPlayers.Count; i++)
	//	{
	//		if(i != slotNumber)
	//			RpcCloseTargetSlot(slotNumber);
	//	}
	//}

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
