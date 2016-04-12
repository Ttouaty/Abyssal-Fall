using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	private ArenaGenerator _generator;
	public GameObject _playerRef;

	// Use this for initialization
	public void Init () {
		_generator = GetComponent<ArenaGenerator>();
		_generator.CreateArena();
		_generator.CreateSpawns();
	}

	public void StartGame ()
	{
		StartCoroutine(CountDown(5));
	}

	private IEnumerator CountDown(int startValue)
	{
		GameManager gm = GameManager.instance;

		for (int s = 0; s < _generator.Spawns.Length; ++s)
		{
			_generator.Spawns[s].SpawnPlayer(s);
		}

		Text countDown = gm.CountdownScreen.transform.FindChild("CountDown").GetComponent<Text>();

		gm.CountdownScreen.SetActive(true);
		while (startValue > 0)
		{
			countDown.text = startValue.ToString();
			startValue--;
			yield return new WaitForSeconds(1);
		}
		gm.CountdownScreen.SetActive(false);

		for(int s = 0; s < _generator.Spawns.Length; ++s)
		{
			_generator.Spawns[s].ActivatePlayer();
		}
	}
}
