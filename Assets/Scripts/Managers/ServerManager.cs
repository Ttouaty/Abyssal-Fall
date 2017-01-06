using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using UnityEngine.Networking.Match;
using UnityEngine.Events;

public class ServerManager : NATTraversal.NetworkManager
{
	[Header("Custom Vars")]
	public static ServerManager Instance;
	private static bool _initialised = false;

	private List<Player> _activePlayers = new List<Player>();
	[HideInInspector]
	public List<Player> RegisteredPlayers { get { return _activePlayers; } }

	private List<Player> _alivePlayers;
	[HideInInspector]
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

	public OpenSlots LobbySlotsOpen = OpenSlots.None;

	public Player HostingClient;

	public bool AreAllPlayerReady
	{
		get
		{
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

		Instance = FindObjectOfType<ServerManager>();
		Instance.GameId = Guid.NewGuid().ToString().Split('-')[0].ToLower();
		_initialised = true;
		return Instance;
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
		if(MenuManager.Instance != null)
		{
			//Prevent Wheels from being destroyed
			NetworkIdentity[] tempIdwheels =  MenuManager.Instance.GetComponentsInChildren<NetworkIdentity>(true);
			for (int i = 0; i < tempIdwheels.Length; i++) 
			{
				NetworkServer.UnSpawn(tempIdwheels[i].gameObject);
			}
		}
	}

	public void TryToAddPlayer()
	{
		if (!NetworkServer.active)
		{
			Debug.Log("is not server, abording player creation");
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
		Debug.Log("client connection detected with adress: " + conn.address);
		if (!NetworkServer.active)
		{
			FindObjectOfType<ConnectionModule>().OnSuccess.Invoke(TargetGameId);
		}
		//base.OnClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (!NetworkServer.active)
		{
			MessageManager.Log("Connection with the host was lost!");
			if (MenuManager.Instance != null)
			{
				MenuManager.Instance.MakeTransition("Main");
			}
			else if (EndGameManager.Instance != null)
			{
				EndGameManager.Instance.ResetGame(false);
			}

			ResetNetwork(true);
		}

		base.OnClientDisconnect(conn);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		Debug.Log("client disconnected with adress => "+conn.address);
		if(IsInLobby && NetworkServer.active) //Remove client authority of disconnected player to avoid wheel destruction
		{
			Debug.Log("Trying to remove wheel authority");

			CharacterSelectWheel[] tempWheels = MenuManager.Instance._characterSlotsContainerRef.GetComponentsInChildren<CharacterSelectWheel>(true);
			for (int i = 0; i < tempWheels.Length; i++)
			{
				if(tempWheels[i].GetComponent<NetworkIdentity>().clientAuthorityOwner == conn)
				{
					Debug.Log("removed authority for => "+tempWheels[i].name);
					tempWheels[i].GetComponent<NetworkIdentity>().RemoveClientAuthority(conn);
					tempWheels[i].GetComponent<NetworkIdentity>().RebuildObservers(true);
					HostingClient.RpcCloseTargetSlot(i);

					if(conn.address != "localServer")
					{
						//external Player decreasing external player count
						ExternalPlayerNumber--;
					}

					ServerRemovePlayer(RegisteredPlayers[i]);
					//NetworkServer.UnSpawn(tempWheels[i].gameObject);
				}
			}
		}

		NetworkServer.DestroyPlayersForConnection(conn);
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
		if (!IsInLobby || RegisteredPlayers.Count >= 4)
		{
			conn.Disconnect();
			Debug.Log("Max players registered !");
			return;
		}

		if(conn.address != "localClient")
		{
			ExternalPlayerNumber++;
		}

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

		Debug.LogWarning("new Player number => "+i);

		if (HostingClient == null) // if that is the first client
			HostingClient = player;


		player.PlayerNumber = i;
		NetworkServer.AddPlayerForConnection(conn, playerGo, playerControllerId);

		//HostingClient.CmdAddPlayerList(player.gameObject);
		HostingClient.RpcOpenSlot(newSlot.ToString(), playerGo, i);
		player.RpcOpenExistingSlots(LobbySlotsOpen.ToString());
		LobbySlotsOpen |= newSlot;
		for (int j = 1; j < RegisteredPlayers.Count; j++)
		{
			RegisteredPlayers[j].RpcOpenSlot(newSlot.ToString(), playerGo, i);
		}
	}

	public void ServerRemovePlayer(Player player)
	{
		//HostingClient.PlayerList.Remove(player.gameObject);
		RegisteredPlayers.Remove(player);
		OpenSlots[] tempArray = Enum.GetValues(typeof(OpenSlots)) as OpenSlots[];
		LobbySlotsOpen &= ~tempArray[player.PlayerNumber];
		Debug.Log("Removed player " + player.PlayerNumber + " - Open slots are now => " + LobbySlotsOpen.ToString());
	}

	public override void OnConnectionReplacedClient(NetworkConnection oldConnection, NetworkConnection newConnection)
	{
		Debug.LogError("Replaced connection");
		ClientScene.AddPlayer(NetworkClient.allClients[0].connection, 0);
		base.OnConnectionReplacedClient(oldConnection, newConnection);
	}

	public override void OnConnectionReplacedServer(NetworkConnection oldConnection, NetworkConnection newConnection)
	{
		Debug.LogError("Replaced connection");
		base.OnConnectionReplacedServer(oldConnection, newConnection);
	}

	public void OnGameEnd()
	{
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
		NetworkServer.SetAllClientsNotReady();
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

	public void ResetNetwork(bool disconnect)
	{
		if(MenuManager.Instance != null)
			MenuManager.Instance.ResetCharacterSelector();

		LobbySlotsOpen = OpenSlots.None;
		_isInGame = false;
		ExternalPlayerNumber = 0;
		HostingClient = null;

		if (NetworkServer.active)
		{
			MasterServer.UnregisterHost();
			StopHost();
		}
		else
			StopClient();

		NetworkServer.Reset();
		NetworkClient.ShutdownAll();

		if (disconnect)
		{
			Network.Disconnect();
		}

		if (matchMaker == null)
			return;
		if (NetworkServer.active)
		{
			matchMaker.DestroyMatch(matchID, 0, OnMatchDropped);
		}
		else
		{
			matchMaker.DropConnection(matchID, matchmakingNodeID, 0, OnMatchDropped);
		}

	}
	//void Update()
	//{
	//	for (int i = 0; i < RegisteredPlayers.Count; i++)
	//	{
	//		Debug.Log("Player n°=> "+ RegisteredPlayers[i].PlayerNumber +" is ready => "+RegisteredPlayers[i].isReady);
	//	}
	//}

	public void ConnectToMatch(string code)
	{
		if (matchMaker == null) matchMaker = gameObject.AddComponent<NetworkMatch>();
		matchMaker.ListMatches(0, 1, "AbyssalFall-" + code, true, 0, 0, OnMatchList);
		TargetGameId = code;
	}


	public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		if(success)
		{
			if(matchList.Count != 0)
			{
				StartClientAll(matchList[0]);
			}
			else
				FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("Failed to find target game. (No Match found)");
		}
		else
		{
			FindObjectOfType<ConnectionModule>().OnFailedConnection.Invoke("Failed to find target game. (Connection error)");
		}
	}

	public IEnumerator GetExternalIP()
	{
		while(!isDoneFetchingExternalIP)
		{
			yield return null;
		}
		
		ExternalIp = externalIP;
	}
}
