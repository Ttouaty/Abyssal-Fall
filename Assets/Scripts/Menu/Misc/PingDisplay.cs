using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PingDisplay : MonoBehaviour
{
	public Text TargetText;
	private int _lastValue;

	public void Activate(Player targetPlayer)
	{
		StopAllCoroutines();

		if(gameObject.activeInHierarchy)
			StartCoroutine(PingCoroutine(targetPlayer));
	}

	IEnumerator PingCoroutine(Player targetPlayer)
	{
		while(true)
		{
			yield return new WaitUntil(() => targetPlayer.Ping != _lastValue);
			TargetText.text = "" + targetPlayer.Ping;
			_lastValue = targetPlayer.Ping;
			yield return new WaitUntil(() => gameObject.activeInHierarchy);
		}
	}
}
