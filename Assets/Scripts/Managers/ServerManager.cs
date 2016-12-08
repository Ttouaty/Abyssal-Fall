using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;

public class ServerManager : NetworkLobbyManager
{
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
	public UnityEventString OnExternalIpRetrieved;

	[HideInInspector]
	public string GameId;
	[HideInInspector]
	public int ExternalPlayerNumber = 0;

	public OpenSlots LobbySlotsOpen = OpenSlots.None;

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
	void Start()
	{
		offlineScene = "";
		lobbyScene = "Scene_Root"; // Name of the scene with the network manager

		originalLobbyScene = lobbyScene;
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
		Init();
	}

	public override void ServerChangeScene(string sceneName)
	{
		// Do nothing
	}

	public override void OnStartClient(NetworkClient lobbyClient)
	{
		lobbyScene = originalLobbyScene; // Ensures the client loads correctly
		Debug.Log("start client");
		base.OnStartClient(lobbyClient);
	}
	public override void OnStopClient()
	{
		lobbyScene = ""; // Ensures we don't reload the scene after quitting
	}

	public override void OnStartServer()
	{
		lobbyScene = originalLobbyScene; // Ensures the server loads correctly
	}
	public override void OnStopServer()
	{
		lobbyScene = ""; // Ensures we don't reload the scene after quitting
		ResetNetwork(true);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{

		Debug.Log("error");
		base.OnClientError(conn, errorCode);
	}
	
	public override void OnClientConnect(NetworkConnection conn)
	{
		Debug.Log("client connection detected with adress: " + conn.address);
		//if (NetworkClient.active)
		//{
		//	Debug.Log("add clientscene player");
		//	ClientScene.AddPlayer(NetworkClient.allClients[0].connection, (short)RegisteredPlayers.Count);
		//}
		base.OnClientConnect(conn);
	}

	public override void OnLobbyClientConnect(NetworkConnection conn)
	{
		Debug.Log("LOBBY CLIENT");
		base.OnLobbyClientConnect(conn);
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		if (Network.connections.Length == 0)
			return;

		if (!Network.isServer)
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
			conn.Disconnect();

		if(conn.address != "localClient")
		{
			ExternalPlayerNumber++;
			Debug.Log("External player connected with address: "+conn.address);
		}

		GameObject player = Instantiate(playerPrefab, transform) as GameObject;
		RegisteredPlayers.Add(player.GetComponent<Player>());

		player.GetComponent<Player>().PlayerNumber = RegisteredPlayers.Count;

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
		NetworkServer.Spawn(player);
		if(LobbySlotsOpen == OpenSlots.None)
			LobbySlotsOpen = OpenSlots.One;
		else
		{
			int i = 0;
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

		/*
		Send currently Open slots + next one
		*/
		player.GetComponent<Player>().RpcOpenTargetSlot(LobbySlotsOpen);
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
	}

	public IEnumerator GetExternalIP()
	{
		if(ExternalIp.Length > 0)
		{
			Debug.Log("IP already retrieved");
			OnExternalIpRetrieved.Invoke(ExternalIp);
			yield break;
		}

		Debug.Log("ExternalIp Retrieved: " + Network.player.externalIP);
		ExternalIp = Network.player.externalIP;
		OnExternalIpRetrieved.Invoke(ExternalIp);
	}
}
