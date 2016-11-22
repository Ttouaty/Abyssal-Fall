using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class ServerManager : NetworkManager
{
	public static ServerManager Instance;
	private static bool _initialised = false;

	private List<Player> _activePlayers = new List<Player>();
	public List<Player> RegisteredPlayers { get { return _activePlayers; } }

	[SerializeField]
	private List<Player> _alivePlayers;
	[HideInInspector]
	public List<Player> AlivePlayers { get { return _alivePlayers; } }

	[HideInInspector]
	public bool IsOnline = false;

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
		Player player = (Player)Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
		RegisteredPlayers.Add(player);
		Debug.Log("add player joystick number here");
		NetworkServer.AddPlayerForConnection(conn, player.gameObject, playerControllerId);
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, UnityEngine.Networking.PlayerController player)
	{
		RegisteredPlayers.Remove(player.gameObject.GetComponent<Player>());
		base.OnServerRemovePlayer(conn, player);
		
	}
}
