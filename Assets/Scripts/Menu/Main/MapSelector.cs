using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MapSelector : MenuPanel
{
	private MapSelectWheel wheelRef;
	public Transform MapWheelTarget;
	protected override void OnEnable()
	{
		if(NetworkServer.active)
		{
			ServerManager.Instance.SpawnMapWheel(Player.LocalPlayer.gameObject);
		}
		base.OnEnable();
	}
}
