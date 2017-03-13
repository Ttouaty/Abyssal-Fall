using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class MapSelector : MenuPanelNew
{
	private MapSelectWheel wheelRef;
	public Transform MapWheelTarget;
	protected void OnEnable()
	{
		if (NetworkServer.active)
		{
			ServerManager.Instance.SpawnMapWheel(Player.LocalPlayer.gameObject);
		}
	}

	protected void OnDisable()
	{
		if (NetworkServer.active)
		{
			Destroy(MapWheelTarget.GetChild(0).gameObject);
		}
	}
}
