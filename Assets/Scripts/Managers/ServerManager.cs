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

	private bool _isInGame = false;
	[HideInInspector]
	public string ExternalIp = "";

	[HideInInspector]
	public string GameId;
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

			ClientScene.RegisterPrefab(tempMapArray[i].Ground); //Add grounds
			ClientScene.RegisterPrefab(tempMapArray[i].Obstacle); //Add obstacle

			for (int j = 0; j < tempMapArray[i].AdditionalPoolsToLoad.Length; j++)
			{
				ClientScene.RegisterPrefab(tempMapArray[i].AdditionalPoolsToLoad[j].Prefab); //Add Additionnal map prefabs
			}
		}

		Instance = FindObjectOfType<ServerManager>();
		Instance.GameId = Guid.NewGuid().ToString().Split('-')[0];
		_initialised = true;
		return Instance;
	}

	private string originalLobbyScene;
	public override void Start()
	{
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
		Init();
		base.Start();
	}

	public override void OnStopServer()
	{
		//lobbyScene = ""; // Ensures we don't reload the scene after quitting
		ResetNetwork(true);
	}

	public void TryToAddPlayer()
	{
		Debug.LogError("TryToAddPlayer");

		if (!NetworkServer.active)
		{
			Debug.Log("is not server, abording player creation");
			return;
		}

		if(NetworkClient.allClients.Count != 0)
		{
			Debug.Log("add player");
			ClientScene.AddPlayer(NetworkClient.allClients[0].connection, (short)ServerManager.Instance.RegisteredPlayers.Count);
		}
		else
		{
			Debug.Log("No client found to add player to! Create a new Client before calling TryToAddPlayer(); if used locally");
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("client connection detected with adress: " + conn.address);
		if (!IsInLobby)
		{
			FindObjectOfType<ConnectionModule>().OnSuccess.Invoke(GameId);
		}

		base.OnClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (!NetworkServer.active)
		{
			Debug.Log("client was disconnected");
			if (MenuManager.Instance != null)
			{
				MenuManager.Instance.MakeTransition("Main");
			}
			else if (EndGameManager.Instance != null)
			{
				EndGameManager.Instance.ResetGame(false);
				MessageManager.Log("Connection with the host was lost!");
			}

			ResetNetwork(true);
		}
		base.OnClientDisconnect(conn);
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		Debug.Log("server side detected connection from: "+conn.address);
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
			Debug.Log("External player connected with address: "+conn.address);
		}

		GameObject playerGo = (GameObject)Instantiate(playerPrefab);

		Player player = playerGo.GetComponent<Player>();
		RegisteredPlayers.Add(player);

		int i = 0;
		if (LobbySlotsOpen == OpenSlots.None)
		{
			i++;
			LobbySlotsOpen = OpenSlots.One;
		}
		else
		{
			foreach (OpenSlots slot in Enum.GetValues(typeof(OpenSlots)))
			{
				if ((LobbySlotsOpen & slot) == 0 && i != 0) //get first unused slot and take it && skip .None
				{
					LobbySlotsOpen |= slot;
					break;
				}
				i++;
			}
		}

		player.PlayerNumber = i;

		if (HostingClient == null) // if that is the first client
			HostingClient = player;

		NetworkServer.AddPlayerForConnection(conn, playerGo, playerControllerId);

		for (int j = 0; j < RegisteredPlayers.Count; j++)
		{
			RegisteredPlayers[j].RpcOpenSlot(LobbySlotsOpen.ToString(), i);
		}
		//HostingClient.RpcOpenSlot(LobbySlotsOpen.ToString(), player.PlayerNumber);
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player)
	{
		RegisteredPlayers.Remove(player.gameObject.GetComponent<Player>());
		OpenSlots[] tempArray = Enum.GetValues(typeof(OpenSlots)) as OpenSlots[];
		LobbySlotsOpen &= ~tempArray[player.gameObject.GetComponent<Player>().PlayerNumber];

		if (IsInLobby)
		{
			player.gameObject.GetComponent<Player>().RpcCloseTargetSlot(player.gameObject.GetComponent<Player>().PlayerNumber -1);
		}
		base.OnServerRemovePlayer(conn, player);
	}

	public void OnGameEnd()
	{
		_isInGame = false;
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
			Debug.Log("launching game (online)");

			_isInGame = true;
			ArenaManager.Instance.RpcAllClientReady();
		}
	}

	private void ResetNetwork(bool disconnect)
	{
		if(disconnect)
			Network.Disconnect();
		LobbySlotsOpen = OpenSlots.None;
		IsInLobby = false;
		_isInGame = false;
		ExternalPlayerNumber = 0;
		HostingClient = null;
		if (NetworkServer.active)
		{
			matchMaker.DestroyMatch(matchID, 0, OnMatchDropped);
		}
		else
		{
			matchMaker.DropConnection(matchID, matchmakingNodeID, 0, OnMatchDropped);
		}

	}

	public void ConnectToMatch(string code)
	{
		if (matchMaker == null) matchMaker = gameObject.AddComponent<NetworkMatch>();
		matchMaker.ListMatches(0, 1, "AbyssalFall-" + code, true, 0, 0, OnMatchList);
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
			yield return true;
		}
		
		ExternalIp = externalIP;
	}
}
