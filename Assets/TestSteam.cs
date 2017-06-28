using UnityEngine;
using System.Collections;
using Steamworks;

public class TestSteam : MonoBehaviour
{

	void Update()
	{
		if(SteamManager.Initialized)
		{
			if(Input.GetKeyDown(KeyCode.Tab))
			{
				if(Input.GetKey(KeyCode.LeftShift))
				{
					//Debug.LogError("Ah! Vous voulez dire que les filles ne savent pas faire une cabane ?!");
				}
			}
		}
	}

	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			Debug.Log("Steam Overlay has been activated");
		}
		else {
			Debug.Log("Steam Overlay has been closed");
		}
	}
}
