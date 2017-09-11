using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking.Match;
using System.Collections.Generic;

public class GetNumberOfPublicGames : MonoBehaviour
{
	public Text TargetText;

	private CanvasGroup _parentCanvasGroup;
	private float _lastCallTimeStamp = 0;
	private float _callInterval = 3;
	private bool _needStart = true;

	private int _maxListSize = 100;

	void Start()
	{
		_parentCanvasGroup = GetComponentInParent<CanvasGroup>();
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

		if (Time.time > _lastCallTimeStamp + _callInterval)
		{
			_lastCallTimeStamp = Time.time;
			if(ServerManager.Instance.matchMaker != null)
				ServerManager.Instance.matchMaker.ListMatches(0, _maxListSize + 1, "-AbyssalFall-Public", true, 0, 0, RandomMatchReturn);
		}
	}
	
	private void RandomMatchReturn(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
	{
		if (success)
		{
			Debug.Log("Received match list: " + matchList.Count + " games found. (max "+ _maxListSize + ")");

			if(matchList.Count > _maxListSize)
				TargetText.text = _maxListSize+"+";
			else
				TargetText.text = ""+matchList.Count;
		}
	}


}
