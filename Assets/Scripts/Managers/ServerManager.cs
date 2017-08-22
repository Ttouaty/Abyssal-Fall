using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.NetworkSystem;
using NATTraversal;

public enum FacilitatorConnectionStatus
{
	Undefined,
	Connected,
	Failed
}

public class CustomMsgTypes
{
	public const short OnConnReplaced = 100;
}

public class ServerManager : NATTraversal.NetworkManager
{
	[Header("Custom Vars")]
	public static ServerManager Instance;
	private static bool _initialised = false;

	private List<Player> _activePlayers = new List<Player>();
	public List<Player> RegisteredPlayers { get { return _activePlayers; } }
	[HideInInspector]
	public List<Player> _alivePlayers;
	public List<Player> AlivePlayers { get { return _alivePlayers; } }

	[HideInInspector]
	public bool IsInLobby = false;

	[HideInInspector]
	public bool _isInGame = false;
	[HideInInspector]
	public string ExternalIp = "";

	[HideInInspector]
	public string GameId;
	[HideInInspector]
	public string TargetGameId;
	[HideInInspector]
	public int ExternalPlayerNumber = 0;

	[HideInInspector]
	public OpenSlots LobbySlotsOpen = OpenSlots.None;
	[HideInInspector]
	public Player HostingClient;

	public bool ForceUnready = false;
	[HideInInspector]
	public bool IsDebug = false;

	[HideInInspector]
	public FacilitatorConnectionStatus FacilitatorStatus = FacilitatorConnectionStatus.Undefined;

	public bool AreAllPlayerReady
	{
		get
		{
			if (ForceUnready)
				return false;
			if (_activePlayers.Count < 2)
				return false;

			for (int i = 0; i < _activePlayers.Count; ++i)
			{
				if (!_activePlayers[i].isReady)
					return false;
			}
			return true;
		}
	}

	public static ServerManager Init()
	{
		if (_initialised)
		{
			return Instance;
		}
		//#########################
		//### ADD SPAWN PREFABS ###
		//#########################

		RegisterPrefabs();

		Instance = FindObjectOfType<ServerManager>();
		string tempId = "";
		
		for (int i = 0; i < 8; i++)
		{
			tempId = tempId + (UnityEngine.Random.Range((int)1, (int)9));
		}
		

		Instance.GameId = tempId;
		_initialised = true;
		return Instance;
	}

	public static void RegisterPrefabs()
	{
		
		PlayerController[] tempPlayerArray = new PlayerController[0];
		DynamicConfig.Instance.GetConfigs(ref tempPlayerArray);
		for (int i = 0; i < tempPlayerArray.Length; i++)
		{
			ClientScene.RegisterPrefab(tempPlayerArray[i].gameObject); //Add Character

			for (int j = 0; j < tempPlayerArray[i]._characterData.OtherAssetsToLoad.Length; j++)
			{
				ClientScene.RegisterPrefab(tempPlayerArray[i]._characterData.OtherAssetsToLoad[j].Prefab); //Add Character Pools
			}
		}

		ArenaConfiguration_SO[] tempMapArray = new ArenaConfiguration_SO[0];
		DynamicConfig.Instance.GetConfigs(ref tempMapArray);

		for (int i = 0; i < tempMapArray.Length; i++)
		{
			if (tempMapArray[i].Ground == null)
				continue;

			//ClientScene.RegisterPrefab(tempMapArray[i].Ground); //Add grounds
			//ClientScene.RegisterPrefab(tempMapArray[i].Obstacle); //Add obstacle

			for (int j = 0; j < tempMapArray[i].AdditionalPoolsToLoad.Length; j++)
			{
				ClientScene.RegisterPrefab(tempMapArray[i].AdditionalPoolsToLoad[j].Prefab); //Add Additionnal map prefabs
			}
		}
	}

	// ######## Start ##########

	public override void Start()
	{
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
		Init();
		base.Start();
	}

	public override void OnServerSceneChanged(string sceneName)
	{
		Debug.LogWarning("Server Change scene detected ! => "+sceneName);
		base.OnServerSceneChanged(sceneName);
	}

	public override void OnStopServer()
	{
		base.OnStopServer();
		//if(MenuManager.Instance != null)
		//{
		//	//Prevent Wheels from being destroyed
		//	NetworkIdentity[] tempIdwheels =  MenuManager.Instance.GetComponentsInChildren<NetworkIdentity>(true);
		//	for (int i = 0; i < tempIdwheels.Length; i++) 
		//	{
		//		NetworkServer.UnSpawn(tempIdwheels[i].gameObject);
		//	}
		//}
	}

	public void TryToAddPlayer()
	{
		if (!NetworkServer.active)
		{
			Debug.Log("is not server, Asking the host to create a new player");
			ClientScene.AddPlayer(client.connection, (short) client.connection.playerControllers.Count);
			//Player.LocalPlayer.CmdTryToAddPlayerFromClient();
			return;
		}

		if(RegisteredPlayers.Count >= 4)
		{
			Debug.LogError("Too much players abording creation");
			return;
		}

		if (NetworkClient.allClients.Count != 0)
		{
			Debug.Log("add player");
			ClientScene.AddPlayer(NetworkClient.allClients[0].connection, (short)ServerManager.Instance.RegisteredPlayers.Count);
		}
		else
		{
			Debug.Log("No client found to add player to! Create a new Client before calling TryToAddPlayer(); if used locally");
		}
	}

	public override void OnStartServer()
	{
		NetworkServer.SpawnObjects();
		base.OnStartServer();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		//base.OnClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (!NetworkServer.active)
		{
			MessageManager.Log("Connection with the host was lost!");
			if (MenuManager.Instance != null)
			{
				MenuPanelNew.PanelRefs["Main"].Open();
			}
			else if (EndGameManager.Instance != null)
			{
				EndGameManager.Instance.ResetGame(false);
			}

			ResetNetwork();
		}

		base.OnClientDisconnect(conn);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("server side detected connection from: "+conn.address);
		if(conn.address == "localServer")
		{
			TryToAddPlayer();
		}
		else
			Debug.Log("external ip waiting for connection replace");

		base.OnServerConnect(conn);
	}

	public static void ResetRegisteredPlayers()
	{
		Instance._activePlayers.Clear();
	}

	public void ResetAlivePlayers()
	{
		_alivePlayers = new List<Player>();

		for (int i = 0; i < _activePlayers.Count; ++i)
		{
			_alivePlayers.Add(_activePlayers[i]);
		}
	}


	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
	{
		Debug.Log("trying to add a new Player");
		if(!IsDebug)
		{
			if (!IsInLobby || RegisteredPlayers.Count >= 4)
			{
				conn.Disconnect();
				if(RegisteredPlayers.Count >= 4)
					Debug.Log("Max players registered !");
				else
					Debug.Log("ServerManager is not in lobby, not accepting any new connection.");
				return;
			}
		}

		if(conn.address != "localClient")
			ExternalPlayerNumber++;

		GameObject playerGo = (GameObject)Instantiate(playerPrefab);

		Player player = playerGo.GetComponent<Player>();
		RegisteredPlayers.Add(player);

		OpenSlots newSlot = OpenSlots.None;
		int i = 0;
		if (LobbySlotsOpen == OpenSlots.None)
		{
			i++;
			newSlot = OpenSlots.One;
		}
		else
		{
			foreach (OpenSlots slot in Enum.GetValues(typeof(OpenSlots)))
			{
				if ((LobbySlotsOpen & slot) == 0 && i != 0) //get first unused slot and take it && skip .None
				{
					newSlot = slot;
					break;
				}
				i++;
			}
		}

		Debug.LogWarning("new Player created, N°=> "+i);

		if (HostingClient == null) // if that is the first client
			HostingClient = player;

		if (MenuManager.Instance == null)
			player.JoystickNumber = 0;
		player.PlayerNumber = i;
		NetworkServer.AddPlayerForConnection(conn, playerGo, playerControllerId);

		LobbySlotsOpen |= newSlot;
	}

	public void SpawnCharacterWheel(GameObject playerObj)
	{
		for (int j = 0; j < spawnPrefabs.Count; j++)
		{
			if (spawnPrefabs[j].GetComponent<CharacterSelectWheel>() != null)
			{
				GameObject newWheel = Instantiate(spawnPrefabs[j]);
				newWheel.GetComponent<CharacterSelectWheel>()._playerRef = playerObj;
				NetworkServer.SpawnWithClientAuthority(newWheel, playerObj);
				break;
			}
			else if (j == spawnPrefabs.Count - 1)
			{
				Debug.LogError("No character wheel found in spawnPrefabs");
				return;
			}
		}
	}

	public void SpawnMapWheel(GameObject playerObj)
	{
		for (int j = 0; j < spawnPrefabs.Count; j++)
		{
			if (spawnPrefabs[j].GetComponent<MapSelectWheel>() != null)
			{
				GameObject newWheel = Instantiate(spawnPrefabs[j]);
				NetworkServer.SpawnWithClientAuthority(newWheel, playerObj);
				break;
			}
			else if (j == spawnPrefabs.Count - 1)
			{
				Debug.LogError("No Map wheel found in spawnPrefabs");
				return;
			}
		}
	}

	public GameObject SpawnObjectOfType<T>(GameObject authorityHolder)
	{
		for (int j = 0; j < spawnPrefabs.Count; j++)
		{
			if (spawnPrefabs[j].GetComponent<T>() != null)
			{
				GameObject newObject = Instantiate(spawnPrefabs[j]);
				if(newObject.GetComponentInChildren<NetworkIdentity>().localPlayerAuthority)
					NetworkServer.SpawnWithClientAuthority(newObject, authorityHolder);
				else
					NetworkServer.Spawn(newObject);

				return newObject;
			}
			else if (j == spawnPrefabs.Count - 1)
			{
				Debug.LogError("No object of type "+typeof(T)+" found in spawnPrefabs");
				return null;
			}
		}
		return null;
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		List<Player> targetPlayers = new List<Player>();

		for (int i = 0; i < conn.playerControllers.Count; i++)
		{
			targetPlayers.Add(conn.playerControllers[i].gameObject.GetComponent<Player>());
		}
		//for (int i = 0; i < RegisteredPlayers.Count; i++)
		//{
		//	if (RegisteredPlayers[i].connectionToClient == conn)
		//	{
		//		targetPlayers.Add(RegisteredPlayers[i]);
		//	}
		//}

		if (targetPlayers.Count == 0)
		{
			//Debug.LogWarning("Player disconnected but no player object was found with playercontrollers => "+ conn.playerControllers.Count);
			return;
		}


		Debug.LogError("players disconnected => "+targetPlayers.Count);

		if (!_isInGame)
		{
			for (int i = 0; i < targetPlayers.Count; i++)
			{
				Player.LocalPlayer.RpcCloseTargetSlot(targetPlayers[i].PlayerNumber - 1);
			}
		}

		NetworkServer.DestroyPlayersForConnection(conn);

		OpenSlots[] tempArray = Enum.GetValues(typeof(OpenSlots)) as OpenSlots[];
		for (int i = 0; i < targetPlayers.Count; i++)
		{
			RegisteredPlayers.Remove(targetPlayers[i]);
			LobbySlotsOpen &= ~tempArray[targetPlayers[i].PlayerNumber];
			Debug.Log("Removed player " + targetPlayers[i].PlayerNumber + " - Open slots are now => " + LobbySlotsOpen.ToString());
		}


		if (conn.address != "localServer")//external Player decreasing external player count
			ExternalPlayerNumber-= targetPlayers.Count;

		
		if(Player.LocalPlayer != null)
		{
			for (int i = 0; i < targetPlayers.Count; i++)
			{
				Player.LocalPlayer.RpcOnPlayerDisconnect(targetPlayers[i].PlayerNumber);
			}
		}

		base.OnServerDisconnect(conn);
	}

	public override void OnConnectionReplacedClient(NetworkConnection oldConnection, NetworkConnection newConnection)
	{
		base.OnConnectionReplacedClient(oldConnection, newConnection);
		newConnection.RegisterHandler(CustomMsgTypes.OnConnReplaced, Instance.OnConnReplaced);	
	}

	public override NetworkConnection checkForAnotherConnectionFromTheSameClient(NetworkConnection con, ConnectionType otherConnectionType = ConnectionType.ANY)
	{
		//Debug.LogError("Other connection type => "+ otherConnectionType+" / other adress ? => " + con.address);
		return base.checkForAnotherConnectionFromTheSameClient(con, otherConnectionType);
	}

	//public override void OnConnectionReplacedServer(NetworkConnection oldConnection, NetworkConnection newConnection)
	//{
	//	//NetworkServer.ReplacePlayerForConnection(newConnection, oldConnection.playerControllers[0].gameObject, 0);
	//	base.OnConnectionReplacedServer(oldConnection, newConnection);
	//}

	public override void replaceConnection(NetworkConnection oldConn, NetworkConnection newConn)
	{
		if(newConn == null || oldConn == null)
		{
			Debug.LogError("Error in replaceConnection() => newConn is "+newConn+" and old conn is "+oldConn);
			if (FindObjectOfType<ConnectionModule>() != null)
				FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("A networking error occurred => a Nat replacement connection occured.");
			else
				MessageManager.Log("A networking error occurred => a Nat replacement connection occured.");
			ResetNetwork();
			return;
		}

		base.replaceConnection(oldConn, newConn);

		if (NetworkServer.active)
			newConn.Send(CustomMsgTypes.OnConnReplaced, new EmptyMessage());
	}

	public void OnConnReplaced(NetworkMessage netMsg)
	{
		FindObjectOfType<ConnectionModule>().OnSuccess.Invoke(TargetGameId);
		//ClientScene.AddPlayer(client.connection, 0);
		MenuManager.Instance.RegisterNewPlayer(InputManager.GetFirstActiveJoystick());
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		base.OnClientError(conn, errorCode);

		if (FindObjectOfType<ConnectionModule>() != null)
			FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("A networking error occurred => " + Enum.GetNames(typeof(NetworkError))[errorCode]);
		else
			MessageManager.Log("A networking error occurred => " + Enum.GetNames(typeof(NetworkError))[errorCode]);

		ResetNetwork();
	}
	
	public void OnGameEnd()
	{
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
	}

	private int _playersReadyForMapSpawn = 0;

	public void AddArenaWaiting()
	{
		if (_isInGame)
		{
			Debug.Log("is already in game");
			return;
		}

		_playersReadyForMapSpawn++;
		if(_playersReadyForMapSpawn > ExternalPlayerNumber) 
		{
			Debug.Log("launching game");

			_isInGame = true;
			HostingClient.RpcAllClientReadyForMapSpawn();
		}
	}

	public void ResetNetwork()
	{
		if(MenuManager.Instance != null)
			MenuManager.Instance.ResetCharacterSelector();

		LobbySlotsOpen = OpenSlots.None;
		_isInGame = false;
		ExternalPlayerNumber = 0;
		HostingClient = null;
		ResetRegisteredPlayers();
		IsDebug = false;

		//if(matchMaker != null)
		//{
		//	if (NetworkServer.active)
		//	{
		//		MasterServer.UnregisterHost();
		//		matchMaker.DestroyMatch(matchID, 0, OnMatchDropped);
		//	}
		//	else
		//		matchMaker.DropConnection(matchID, matchmakingNodeID, 0, OnMatchDropped);
		//}

		if (NetworkServer.active)
		{
			StopHost();
			client.Shutdown();
		}
		else
		{
			StopClient();
			//for (int i = 0; i < NetworkClient.allClients.Count; i++)
			//{
			//	if(NetworkClient.allClients[i].connection != null)
			//	{
			//		for (int j = 0; j < NetworkClient.allClients[i].connection.playerControllers.Count; j++)
			//		{
			//			Destroy(NetworkClient.allClients[i].connection.playerControllers[j].gameObject);
			//		}
			//	}
			//}
			NetworkClient.ShutdownAll();
		}
		//for (int i = 0; i < NetworkClient.allClients.Count; i++)
		//{
		//	NetworkServer.DestroyPlayersForConnection(NetworkClient.allClients[i].connection);
		//	NetworkClient.allClients[i].Disconnect();

		//	NetworkClient.allClients[i].Shutdown();
		//}
		//StopClient();

		RegisterPrefabs();
	}

	public override void Update()
	{
		base.Update();

		//if (NetworkServer.active)
		//{
		//	for (int i = 0; i < RegisteredPlayers.Count; i++)
		//	{
		//		Debug.Log("Player n°=> " + RegisteredPlayers[i].PlayerNumber + " connection obj => " + RegisteredPlayers[i].connectionToClient.playerControllers.Count);
		//	}
		//}
	}

	public void ConnectToMatch(string code)
	{
		if (matchMaker == null) matchMaker = gameObject.AddComponent<NetworkMatch>();
		matchMaker.ListMatches(0, 1, "AbyssalFall-" + code, true, 0, 0, OnMatchList);
		TargetGameId = code;
	}

	public void FreezeAllPlayers()
	{
		for (int i = 0; i < RegisteredPlayers.Count; ++i)
		{
			RegisteredPlayers[i].Controller.RpcFreeze();
		}
	}

	public void UnFreezeAllPlayers()
	{
		for (int i = 0; i < RegisteredPlayers.Count; ++i)
		{
			RegisteredPlayers[i].Controller.RpcUnFreeze();
		}
	}

	public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		if(success)
		{
			if(FindObjectOfType<ConnectionModule>() != null)
			{
				if(matchList.Count != 0 && FindObjectOfType<ConnectionModule>().IsConnecting)
				{
					StartCoroutine("MatchListTimeOut");
					StartClientAll(matchList[0]);
				}
				else
					FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("Failed to find target game. (No Match found)");
			}
		}
		else
		{
			FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("Failed to find target game. (Connection error)");
		}
	}

	IEnumerator MatchListTimeOut()
	{
		float timeoutTime = 20;
		yield return new WaitForSeconds(timeoutTime);
		if (Player.LocalPlayer == null)
		{
			ResetNetwork();
			ConnectionModule tempCoModule = FindObjectOfType<ConnectionModule>();
			if(tempCoModule != null)
				tempCoModule.OnFailedConnection.Invoke("Connection attempt failed after " + timeoutTime + "s, aborting connection.");
		}
		else
			Debug.Log("TimeOut not activated, player is here => ok");
	}

	public override void OnDoneConnectingToFacilitator(ulong guid)
	{
		base.OnDoneConnectingToFacilitator(guid);
		if(guid == 0)
		{
			FacilitatorStatus = FacilitatorConnectionStatus.Failed;
			MessageManager.Log("Could not connect to our facilitator server.\n(you can still play locally)", 10);
		}
		else
			FacilitatorStatus = FacilitatorConnectionStatus.Connected;
	}
}
