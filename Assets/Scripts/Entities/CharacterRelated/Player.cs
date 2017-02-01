using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class Player : NetworkBehaviour
{
	public static Player LocalPlayer;
	private static PlayerController[] _availablePlayerControllers;

	//[SyncVar]
	//public SyncListGameObject PlayerList = new SyncListGameObject();
	public static Player[] PlayerList = new Player[0];
	public Material CharacterAlpha;
	[HideInInspector]
	public int JoystickNumber = 0;

	[SyncVar]
	[HideInInspector]
	public int Score = 0;

	public bool isReady
	{
		get { return _ready; }
		private set { _ready = value; }
	}

	[SyncVar(hook = "SetWheelReady")]
	private bool _ready;
	[HideInInspector]
	[SyncVar]
	public int SkinNumber = 0; //the index of the material used by the playerMesh
	[HideInInspector]
	[SyncVar]
	public int PlayerNumber;
	[HideInInspector]
	public PlayerController Controller;// PlayerController Instantiated

	// PlayerController Prefab Model index
	[SyncVar]
	public int CharacterUsedIndex = 0;
	public PlayerController CharacterUsed
	{
		get
		{
			if (_availablePlayerControllers == null)
				DynamicConfig.Instance.GetConfigs(ref _availablePlayerControllers);

			return _availablePlayerControllers[CharacterUsedIndex].GetComponent<PlayerController>();
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

	public void Ready(int characterIndex, int indexSkinUsed)
	{
		_ready = true;
		CmdReadyPlayer(characterIndex, indexSkinUsed);
	}

	public void SetWheelReady(bool ready)
	{
		_ready = ready;
		MenuManager.Instance._characterSlotsContainerRef.SlotsAvailable[PlayerNumber - 1].SelectPedestal(ready);
	}

	[Command]
	public void CmdReadyPlayer(int characterIndex, int indexSkinUsed)
	{
		_ready = true;
		CharacterUsedIndex = characterIndex;
		SkinNumber = indexSkinUsed;
		Debug.Log("Player N°" + PlayerNumber + " has selected (name)=> " + CharacterUsed._characterData.IngameName);

	}

	[Command]
	public void CmdSetPlayerCharacter(int characterIndex, int indexSkinUsed)
	{
		CharacterUsedIndex = characterIndex;
		SkinNumber = indexSkinUsed;
	}

	public void UnReady()
	{
		_ready = false;
		if(!NetworkServer.active)
			CmdUnReadyPlayer();
	}

	[Command]
	public void CmdUnReadyPlayer()
	{
		_ready = false;
	}

	void OnDestroy()
	{
		if (NetworkServer.active)
		{
			if (Controller != null && isLocalPlayer)
				Destroy(Controller.gameObject);
		}
		enabled = false;
		PlayerList = FindObjectsOfType<Player>();
		if (PlayerList.Length == 1)
		{
			if(NetworkServer.active && ServerManager.Instance._isInGame)
			{
				Debug.Log("Is last player remaining in game, going back to main menu");
				if(EndGameManager.Instance != null)
					EndGameManager.Instance.ResetGame(false);
			}
		}
	}

	public override void OnNetworkDestroy()
	{
		if (MenuManager.Instance != null && isLocalPlayer)
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
			CmdUpdatePlayerList();
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
					JoystickNumber = MenuManager.Instance.LocalJoystickBuffer[MenuManager.Instance.LocalJoystickBuffer.Count - 1];
					Debug.Log("player " + name + " created with joystick number: " + JoystickNumber);
				}
			}
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		CmdUpdatePlayerList();
	}

	void Update()
	{
		if(isLocalPlayer)
		{
			if(!NetworkServer.active && JoystickNumber == 0)
			{
				int newJoystick = InputManager.AnyButtonDown(true);
				if (newJoystick != -1)
				{
					MessageManager.Log("Joystick Detected.\nOverriding keyboard controls.");
					JoystickNumber = newJoystick;
				}
			}
		}
	}

	[Command]
	private void CmdUpdatePlayerList(){ RpcUpdatePlayerList(); }
	[ClientRpc]
	private void RpcUpdatePlayerList() { PlayerList = FindObjectsOfType<Player>(); }

	[ClientRpc]
	public void RpcCloseTargetSlot(int slotNumber)
	{
		MenuManager.Instance.CloseCharacterSlot(slotNumber);
	}

	[ClientRpc]
	public void RpcMenuTransition(string newMenuName, bool dir)
	{
		if (isLocalPlayer)
			return; // activate only if not caller

		if(dir)
			MenuPanelNew.PanelRefs[newMenuName].Open();
		else
			MenuPanelNew.PanelRefs[newMenuName].Return();
	}

	[ClientRpc]
	public void RpcStartGame(GameConfiguration newGameConfig, ParsedGameRules customRules)
	{
		Debug.Log("Player N°=> " + PlayerNumber + " is starting game with config.");
		PlayerList = FindObjectsOfType<Player>();
		Array.Sort(PlayerList, (Player x, Player y) => { return x.PlayerNumber.CompareTo(y.PlayerNumber); });
		GameManager.Instance.StartGameWithConfig(newGameConfig, customRules);
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
		if (index == -1)
		{
			Debug.Log("tile index was -1");
			return;
		}

		if (NetworkServer.active)
		{
			ArenaMasterManager.Instance.RpcRemoveTileAtIndex(index);
		}
	}

	[ClientRpc]
	public void RpcInitController(GameObject targetObject)
	{
		if (targetObject == null)
			Debug.LogError("RPC init >targetObject< was null !");


		targetObject.GetComponent<PlayerController>().Init(gameObject);

		for (int i = 0; i < PlayerList.Length; i++)
		{
			if(PlayerList[i].PlayerNumber != PlayerNumber)
			{
				if (PlayerList[i].SkinNumber == SkinNumber && PlayerList[i].CharacterUsedIndex == CharacterUsedIndex)
				{
					Debug.Log("Player N°=> "+PlayerNumber+" is adding differentialAlpha to player => "+targetObject.GetComponent<PlayerController>()._characterData.IngameName);
					targetObject.GetComponent<PlayerController>().AddDifferentialAlpha(CharacterAlpha);
				}
			}
		}
	}

	[ClientRpc]
	public void RpcOnPlayerWin(GameObject winnerPlayerGo)
	{
		Player winner = null;
		if (winnerPlayerGo != null)
			winner = winnerPlayerGo.GetComponent<Player>();

		Debug.Log("RpcOnPlayerWin received on player n°=> " + PlayerNumber + " with localplayer n°=> " + LocalPlayer.PlayerNumber);
		GameManager.Instance.OnPlayerWin.Invoke(winner);
	}

	[ClientRpc]
	public void RpcResetmap(bool animate)
	{
		ArenaManager.Instance.ResetMap(animate);
		EndStageManager.Instance.Close();
	}
	
	[ClientRpc]
	public void RpcOpenMenu(bool showSplashScreens, string targetMenuName, bool openCharacterSelect)
	{
		MainManager.Instance.LEVEL_MANAGER.OpenMenu(showSplashScreens, targetMenuName, openCharacterSelect);
	}
}