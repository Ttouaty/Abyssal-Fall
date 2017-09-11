using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;

public class Player : NetworkBehaviour
{
	public static Player LocalPlayer;
	private static PlayerController[] _availablePlayerControllers;
	public static PlayerController[] AvailablePlayerControllers
	{
		get
		{
			if (_availablePlayerControllers == null)
				DynamicConfig.Instance.GetConfigs(ref _availablePlayerControllers);
			return _availablePlayerControllers;
		}
	}

	public static Player GetPlayerWithNumber(int playerNumber)
	{
		return PlayerList.First((Player p) => p.PlayerNumber == playerNumber);
	}

	public static Player[] PlayerList = new Player[0];
	[HideInInspector]
	public int JoystickNumber = -1;
	[SyncVar]
	[HideInInspector]
	public bool IsUsingGamePad = false;

	[SyncVar(hook = "OnScoreUpdate")]
	[HideInInspector]
	public float Score = 0;

	public bool isReady
	{
		get { return _ready; }
		private set { _ready = value; }
	}

	public Color PlayerColor
	{
		get
		{
			return GameManager.Instance.PlayerColors[PlayerNumber - 1];
		}
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

	private float _targetPingPerSecond = 1.5f;
	[SyncVar]
	public int Ping = 0;
	[HideInInspector]
	public Sprite Icon; //Used by bots 

	public void SelectCharacter(ref PlayerController newCharacter)
	{
		newCharacter._playerRef = this;
	}

	public void Ready(int characterIndex, int indexSkinUsed)
	{
		_ready = true;
		CmdReadyPlayer(characterIndex, indexSkinUsed);
	}

	public void SetWheelReady(bool ready)
	{
		_ready = ready;

		ChangeWheelState(ready);
	}

	public void OnScoreUpdate(float newScore)
	{
		if (newScore > Score && Controller != null)
		{
			SoundManager.Instance.PlayOS("Score");
			if (newScore - Score == 1)
				Instantiate(GameManager.Instance.Popups["+1"], Controller.transform.position + Vector3.up * 2, Camera.main.transform.rotation);
			if (newScore - Score == 2)
				Instantiate(GameManager.Instance.Popups["+2"], Controller.transform.position + Vector3.up * 2, Camera.main.transform.rotation);
			if (newScore - Score == 3)
				Instantiate(GameManager.Instance.Popups["+3"], Controller.transform.position + Vector3.up * 2, Camera.main.transform.rotation);
		}

		Score = newScore;
	}

	public void ChangeWheelState(bool ready)
	{
		if (CharacterSelectWheel.WheelsRef.ContainsKey(PlayerNumber))
		{
			if (CharacterSelectWheel.WheelsRef[PlayerNumber] == null)
				return;

			CharacterSelectWheel.WheelsRef[PlayerNumber].GetComponentInParent<CharacterSlot>().SelectPedestal(ready);
			CharacterSelectWheel.WheelsRef[PlayerNumber].SetAnimBool("IsSelected", ready);
		}
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
		if (!NetworkServer.active)
			CmdUnReadyPlayer();
	}

	[Command]
	public void CmdUnReadyPlayer()
	{
		_ready = false;
	}

	void OnDestroy()
	{
		PlayerList = FindObjectsOfType<Player>();
		if (NetworkServer.active)
		{
			if (Controller != null && isLocalPlayer)
				Destroy(Controller.gameObject);

			if (GameManager.Instance != null)
			{
				if (GameManager.Instance.GameRules != null)
				{
					if (PlayerList.Length < GameManager.Instance.GameRules.NumberOfCharactersRequired)
					{
						MessageManager.Log("Not Enough Characters to continue. Returning to Main Menu");
						if (EndGameManager.Instance != null)
							EndGameManager.Instance.ResetGame(false);
					}
				}
			}
		}
	}

	public override void OnStartLocalPlayer()
	{
		if (isLocalPlayer)
		{
			LocalPlayer = this;

			CmdUpdatePlayerList();
			ChangeWheelState(_ready);
			CmdSpawnCharacterSelectWheel();

			if (MenuManager.Instance != null)
			{
				if (MenuManager.Instance.LocalJoystickBuffer.Count != 0)
				{
					JoystickNumber = MenuManager.Instance.LocalJoystickBuffer[MenuManager.Instance.LocalJoystickBuffer.Count - 1];
					MenuManager.Instance.LocalJoystickBuffer.RemoveAt(MenuManager.Instance.LocalJoystickBuffer.Count - 1);

					Debug.Log("player N°" + PlayerNumber + " created with joystick number: " + JoystickNumber);
				}
			}

			//if (!NetworkServer.active)
			//{
			//	ServerManager.Instance.StopCoroutine("MatchListTimeOut");
			//	string[] tempJoystickNames = InputManager.GetJoystickNames();
			//	if (tempJoystickNames.Length > 1)
			//	{
			//		Debug.Log("InputManager.GetJoystickNames().Length > 1 == true ! Replacing with last connected joystick");

			//		for (int i = tempJoystickNames.Length - 1; i >= 0; i--)
			//		{
			//			if (tempJoystickNames[i].Length != 0)
			//			{
			//				JoystickNumber = i;
			//				break;
			//			}
			//		}

			//		Debug.Log("new joystick number is => " + JoystickNumber + " / name => " + tempJoystickNames[JoystickNumber]);
			//	}
			//}

			CmdSetIsUsingGamePad(JoystickNumber != 0);
		}
	}

	public void StartCheckingForPing()
	{
		StartCoroutine(PingCoroutine());
	}

	IEnumerator PingCoroutine()
	{
		while (NetworkServer.active)
		{
			for (int j = 0; j < PlayerList.Length; j++)
			{
				if(PlayerList[j].isLocalPlayer)
				{
					Ping = 0;
					continue;
				}
				byte error;
				if(PlayerList[j].connectionToClient != null)
					PlayerList[j].Ping = NetworkTransport.GetCurrentRtt(PlayerList[j].connectionToClient.hostId, PlayerList[j].connectionToClient.connectionId, out error);

				Debug.Log("NetworkTransport ping => " + PlayerList[j].Ping);	
			}

			float timePinged = Time.time;

			yield return new WaitUntil(() => Time.time > timePinged + (1 / _targetPingPerSecond));

			//if(isLocalPlayer)
			//	Ping = 0;
			//else
			//	Ping = Network.GetAveragePing(Network.player);

			//Debug.Log("GOT PING => "+ Ping +" for player n° => "+PlayerNumber +" & is server => "+isServer);
		}
	}

	[Command]
	private void CmdSetIsUsingGamePad(bool value)
	{
		IsUsingGamePad = value;
	}

	[Command]
	private void CmdSpawnCharacterSelectWheel()
	{
		if (MenuManager.Instance != null && !ServerManager.Instance.IsDebug)
			ServerManager.Instance.SpawnCharacterWheel(gameObject);
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
	}

	void Update()
	{
		//if(isLocalPlayer)
		//{
		//	if(!NetworkServer.active && JoystickNumber == 0)
		//	{
		//		int newJoystick = InputManager.AnyButtonDown(true);
		//		if (newJoystick != 0)
		//		{
		//			MessageManager.Log("Joystick Detected.\nOverriding keyboard controls.");
		//			JoystickNumber = newJoystick;
		//		}
		//	}
		//}
	}

	[Command]
	private void CmdUpdatePlayerList() { RpcUpdatePlayerList(); }
	[ClientRpc]
	private void RpcUpdatePlayerList() { PlayerList = FindObjectsOfType<Player>().Where((Player p) => { return p.PlayerNumber > 0; }).OrderBy((Player p) => { return PlayerNumber; }).ToArray(); }

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

		if (dir)
			MenuPanelNew.PanelRefs[newMenuName].Open();
		else
			MenuPanelNew.PanelRefs[newMenuName].Return();
	}

	[ClientRpc]
	public void RpcStartGame(GameConfiguration newGameConfig, int[] customRules)
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
			ArenaMasterManager.Instance.RpcRemoveTile(index);
		}
	}

	[ClientRpc]
	public void RpcInitController(GameObject targetObject)
	{
		targetObject.GetComponent<PlayerController>().Init(gameObject);
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
	public void RpcOnRoundEnd(GameObject winnerPlayerGo)
	{
		Player winner = null;
		if (winnerPlayerGo != null)
			winner = winnerPlayerGo.GetComponent<Player>();

		Debug.Log("RpcOnRoundEnd received on player n°=> " + PlayerNumber + " with localplayer n°=> " + LocalPlayer.PlayerNumber);
		GameManager.Instance.OnRoundEnd.Invoke(winner);
	}

	[ClientRpc]
	public void RpcResetmap(bool animate, int targetMapIndex)
	{
		GameManager.Instance.CurrentGameConfiguration.MapFileUsedIndex = targetMapIndex;
		ArenaManager.Instance.ResetMap(animate);
		EndStageManager.Instance.Close();
	}

	[ClientRpc]
	public void RpcOpenMenu(bool showSplashScreens, string targetMenuName, bool openCharacterSelect)
	{
		MainManager.Instance.LEVEL_MANAGER.OpenMenu(showSplashScreens, targetMenuName, openCharacterSelect);
	}


	[ClientRpc]
	public void RpcOnPlayerDisconnect(int playerNumber)
	{
		BroadcastMessage("OnPlayerDisconnect", playerNumber, SendMessageOptions.DontRequireReceiver);
	}

	[ClientRpc]
	public void RpcToggleNoClip()
	{
		//Debug.LogError("NoClip toggled from server !");
		GroundCheck.noclip = !GroundCheck.noclip;
		MessageManager.Log("Toggled Noclip to => " + GroundCheck.noclip);
	}

	[ClientRpc]
	public void RpcShakeCam(ShakeStrength force)
	{
		CameraManager.Shake(force);
	}

	[ClientRpc]
	public void RpcSuddenDeath(GameObject[] playersSuddenDeath, GameConfiguration newConfig)
	{
		GameManager.Instance.PreviousGameConfig = GameManager.Instance.CurrentGameConfiguration;

		GameManager.Instance.CurrentGameConfiguration = newConfig;

		MainManager.Instance.DYNAMIC_CONFIG.GetConfig(newConfig.ArenaConfiguration, out LevelManager.Instance.CurrentArenaConfig);
		MainManager.Instance.DYNAMIC_CONFIG.GetConfig(newConfig.ModeConfiguration, out LevelManager.Instance.CurrentModeConfig);
		MainManager.Instance.DYNAMIC_CONFIG.GetConfig(newConfig.MapConfiguration, out LevelManager.Instance.CurrentMapConfig);


		StartCoroutine(GameManager.Instance.GameRules.SuddenDeath(playersSuddenDeath));
		GameManager.Instance.CurrentGameConfiguration = GameManager.Instance.PreviousGameConfig;
	}

	public void PlaySoundForAll(string fmodKey)
	{
		if (NetworkServer.active)
			RpcPlaySound(fmodKey);
		else
			CmdPlaySound(fmodKey);
	}

	[Command]
	public void CmdPlaySound(string fmodKey) { RpcPlaySound(fmodKey); }
	[ClientRpc]
	public void RpcPlaySound(string fmodKey) { FMODUnity.RuntimeManager.PlayOneShot(fmodKey); }
	[ClientRpc]
	public void RpcReturnToCharacterSelect() { EndGameManager.Instance.ReturnToCharacterSelectFromRpc(); }
	[ClientRpc]
	public void RpcDisplayKill(int killerPlayerNumber, int victimPlayerNumber) { GUIManager.Instance.DisplayKill(killerPlayerNumber, victimPlayerNumber); }
	[ClientRpc]
	public void RpcDisplaySuicide(int victimPlayerNumber) { GUIManager.Instance.DisplaySuicide(victimPlayerNumber); }
	[ClientRpc]
	public void RpcDisplayEnvironnementKill(GameObject killer, int victimPlayerNumber) { GUIManager.Instance.DisplayEnvironnementKill(killer.GetComponent<Player>(), victimPlayerNumber); }
	
}