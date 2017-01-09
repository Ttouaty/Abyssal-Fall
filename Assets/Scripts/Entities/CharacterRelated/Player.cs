﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class Player : NetworkBehaviour
{
	public static Player LocalPlayer;
	private static PlayerController[] _availablePlayerControllers;

	//[SyncVar]
	//public SyncListGameObject PlayerList = new SyncListGameObject();
	public Player[] PlayerList = new Player[0];

	[HideInInspector]
	public int JoystickNumber = 0;

	[SyncVar]
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
	[SyncVar]
	[HideInInspector]
	public int PlayerNumber;
	[HideInInspector]
	public PlayerController Controller;// PlayerController Instantiated

	// PlayerController Prefab Model index
	[SyncVar]
	private int _characterUsedIndex = -1;
	public PlayerController CharacterUsed
	{
		get
		{
			if (_characterUsedIndex == -1)
			{
				Debug.LogError("Character used index is -1 !");
				return null;
			}
			if (_availablePlayerControllers == null)
				DynamicConfig.Instance.GetConfigs(ref _availablePlayerControllers);

			return _availablePlayerControllers[_characterUsedIndex].GetComponent<PlayerController>();
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

	public void Ready(int characterIndex, int indexSkinUsed)
	{
		_ready = true;
		CmdReadyPlayer(characterIndex, indexSkinUsed);
	}

	[Command]
	public void CmdReadyPlayer(int characterIndex, int indexSkinUsed)
	{
		_ready = true;
		_characterUsedIndex = characterIndex;
		SkinNumber = indexSkinUsed;
		Debug.Log("PLayer N°" + PlayerNumber + " has selected (name)=> " + CharacterUsed._characterData.IngameName);

	}

	public void UnReady()
	{
		_characterUsedIndex = -1;
		_ready = false;
		CmdUnReadyPlayer();
	}

	[Command]
	public void CmdUnReadyPlayer()
	{
		_ready = false;
		_characterUsedIndex = -1;
		SkinNumber = 0;
	}

	void OnDestroy()
	{
		if (NetworkServer.active)
		{
			if (Controller != null && isLocalPlayer)
				Destroy(Controller.gameObject);
		}
		enabled = false;

		LocalPlayer.PlayerList = FindObjectsOfType<Player>();
		if(LocalPlayer.PlayerList.Length == 1)
		{
			Debug.Log("Is last player remaining in game");
		}

	}

	public override void OnNetworkDestroy()
	{
		if(MenuManager.Instance != null && isLocalPlayer)
		{
			CharacterSelectWheel[] tempWheels = MenuManager.Instance.GetComponentsInChildren<CharacterSelectWheel>();
			for (int i = 0; i < tempWheels.Length; i++)
			{
				if (tempWheels[i].GetComponent<NetworkIdentity>().clientAuthorityOwner == connectionToServer)
				{
					Debug.Log("removed authority for => " + tempWheels[i].name);
					tempWheels[i].GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToServer);
					break;
				}
			}
			RpcCloseTargetSlot(PlayerNumber - 1);
		}
		base.OnNetworkDestroy();
	}

	public override void OnStartLocalPlayer()
	{
		if (isLocalPlayer)
		{
			LocalPlayer = this;

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
					Debug.Log("openning slots: " + slot + " with player null");
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
			{
				PlayerNumber = newPlayerNumber;
				name += "_" + PlayerNumber;
			}
			PlayerList = FindObjectsOfType<Player>();
			OpenSlots NewSlotToOpen = (OpenSlots)Enum.Parse(typeof(OpenSlots), slotToOpen);
			MenuManager.Instance.OpenCharacterSlot(NewSlotToOpen, OwnerPlayer.GetComponent<Player>()); //open his own slot
		}
	}

	[ClientRpc]
	public void RpcCloseTargetSlot(int slotNumber)
	{
		MenuManager.Instance.CloseCharacterSlot(slotNumber);
	}

	
	[ClientRpc]
	public void RpcMenuTransition(string newMenuName, bool dir)
	{
		if(isLocalPlayer)
		{
			Debug.Log("Making transition on player with netId => "+netId);
			MenuManager.Instance.MakeTransition(newMenuName, dir, false);
		}
	}

	[ClientRpc]
	public void RpcStartGame(GameConfiguration newGameConfig)
	{
		if(isLocalPlayer)
		{
			Debug.Log("Player N°=> "+PlayerNumber+" is starting game with config.");
			GameManager.Instance.StartGameWithConfig(newGameConfig);
		}
	}

	[Command]
	public void CmdReadyToSpawnMap()
	{
		ServerManager.Instance.AddArenaWaiting();
	}

	[ClientRpc]
	public void RpcAllClientReadyForMapSpawn()
	{
		ArenaManager.Instance.ResetMap(true);
	}

	[Command]
	public void CmdRemoveTile(int index)
	{
		if(index == -1)
		{
			Debug.Log("tile index was -1");
			return;
		}

		if(NetworkServer.active)
		{
			ArenaMasterManager.Instance.RpcRemoveTileAtIndex(index);
		}
	}

	[ClientRpc]
	public void RpcInitController(GameObject targetObject)
	{
		if(targetObject == null)
			Debug.LogError("RPC init >targetObject< was null !");

		Debug.Log(targetObject.GetComponent<PlayerController>());
		targetObject.GetComponent<PlayerController>().Init(gameObject);
	}

	[ClientRpc]
	public void RpcOnPlayerWin(GameObject winnerPlayerGo)
	{
		Player winner = null;
		if (winnerPlayerGo != null)
			winner = winnerPlayerGo.GetComponent<Player>();

		Debug.Log("RpcOnPlayerWin received on player n°=> "+PlayerNumber+" with localplayer n°=> "+LocalPlayer.PlayerNumber);
		GameManager.Instance.OnPlayerWin.Invoke(winner);
	}

	[ClientRpc]
	public void RpcResetmap(bool animate)
	{
		ArenaManager.Instance.ResetMap(animate);
		EndStageManager.Instance.Close();
	}

	//[Command]
	//public void CmdAddPlayerList(GameObject obj)
	//{
	//	PlayerList.Add(obj);
	//}

	//[Command]
	//public void CmdRemovePlayerList(GameObject obj)
	//{
	//	PlayerList.Remove(obj);
	//}
}