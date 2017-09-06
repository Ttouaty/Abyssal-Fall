using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Steamworks;

public class GetPlayersOnline : MonoBehaviour
{
	public Text TargetText;

	private CanvasGroup _parentCanvasGroup;
	private float _lastCallTimeStamp = 0;
	private float _callInterval = 3;
	private bool _needStart = true;

	private CallResult<NumberOfCurrentPlayers_t> OnReceivedNumberOfPlayerCallBack;
	
	void Start()
	{
		_parentCanvasGroup = GetComponentInParent<CanvasGroup>();
		OnReceivedNumberOfPlayerCallBack = new CallResult<NumberOfCurrentPlayers_t>(OnReceivedNumberOfPlayer);
	}

	void OnActivate()
	{
		TargetText.text = "-";
		_lastCallTimeStamp = Time.time - _callInterval + 0.5f;
	}

	void Update()
	{
		if (_parentCanvasGroup.alpha == 0)
		{
			_needStart = true;
			return;
		}
		else if (_needStart)
		{
			_needStart = false;
			OnActivate();
		}

		if (!SteamManager.Initialized)
			return;
		
		if(Time.time > _lastCallTimeStamp + _callInterval)
		{
			_lastCallTimeStamp = Time.time;
			CallForPlayerOnline();
		}
	}

	public void CallForPlayerOnline()
	{
		OnReceivedNumberOfPlayerCallBack.Set(SteamUserStats.GetNumberOfCurrentPlayers());
	}

	private void OnReceivedNumberOfPlayer(NumberOfCurrentPlayers_t numPlayers, bool bIOFailure)
	{
		if (bIOFailure || numPlayers.m_bSuccess == 0)
		{
			Debug.Log("NumberOfCurrentPlayers_t failed!\n");
			return;
		}

		TargetText.text = ""+numPlayers.m_cPlayers + 1;
	}
}
