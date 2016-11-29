using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ServerManager : NetworkLobbyManager
{
	public static ServerManager Instance;
	private static bool _initialised = false;

	private List<Player> _activePlayers = new List<Player>();
	[HideInInspector]
	public List<Player> RegisteredPlayers { get { return _activePlayers; } }


	[SerializeField]
	private List<Player> _alivePlayers;
	[HideInInspector]
	public List<Player> AlivePlayers { get { return _alivePlayers; } }

	[HideInInspector]
	public bool IsOnline = false;
	[HideInInspector]
	public bool IsInLobby = false;

	private bool _isInGame = false;

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
			Debug.Log("ServerManager was already Initialised.");
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

		_initialised = true;
		return Instance;
	}



	private string originalLobbyScene;
	public override void ServerChangeScene(string sceneName)
	{
		// Do nothing

		Debug.Log("ass");
	}

	void Start()
	{
		offlineScene = "";
		lobbyScene = "Scene_Root"; // Name of the scene with the network manager
		originalLobbyScene = lobbyScene;
		_isInGame = false;
		_playersReadyForMapSpawn = 0;
	}
	public override void OnStartClient(NetworkClient lobbyClient)
	{
		lobbyScene = originalLobbyScene; // Ensures the client loads correctly
	}
	public override void OnStopClient()
	{
		lobbyScene = ""; // Ensures we don't reload the scene after quitting
	}
	public override void OnStartServer()
	{
		NetworkServer.SetAllClientsNotReady();
		lobbyScene = originalLobbyScene; // Ensures the server loads correctly
	}
	public override void OnStopServer()
	{
		lobbyScene = ""; // Ensures we don't reload the scene after quitting
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
		if (GameManager.InProgress)
			conn.Dispose();

		GameObject player = Instantiate(playerPrefab, transform) as GameObject;
		RegisteredPlayers.Add(player.GetComponent<Player>());

		player.GetComponent<Player>().PlayerNumber = RegisteredPlayers.Count;

		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

		for (int i = 0; i < RegisteredPlayers.Count; i++)
		{
			RegisteredPlayers[i].RpcOpenTargetSlot(player.GetComponent<Player>().PlayerNumber -1);
		}
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player)
	{
		RegisteredPlayers.Remove(player.gameObject.GetComponent<Player>());
		if (IsInLobby)
		{
			for (int i = 0; i < RegisteredPlayers.Count; i++)
			{
				RegisteredPlayers[i].RpcCloseTargetSlot(player.gameObject.GetComponent<Player>().PlayerNumber -1);
			}
		}
		base.OnServerRemovePlayer(conn, player);
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
		if(!IsOnline)
		{
			Debug.Log("launching game (normal)");
			_isInGame = true;
			ArenaManager.Instance.RpcAllClientReady();
		}
		else if(_playersReadyForMapSpawn >= RegisteredPlayers.Count) 
		{
			Debug.Log("launching game (online)");

			_isInGame = true;
			ArenaManager.Instance.RpcAllClientReady();
		}
	}
}
