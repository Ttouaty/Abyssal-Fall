using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	public static Arena instance;

	public static float TileScale
	{
		get
		{
			return instance._generator.TileScale;
		}
	}

	private ArenaGenerator _generator;
	private bool _loaded;

	void Awake ()
	{
		instance = this;
		_loaded = false;
		_generator = GetComponent<ArenaGenerator>();
	}

	void Start ()
	{
		GameManager.instance.OnPlayerWin.AddListener(OnPlayerWin);
	}

	// Use this for initialization
	public IEnumerator StartGame()
	{
		// Start initialisation of arena field
		StopAllCoroutines();
		_generator.ResetStage();
		_generator.CreateArena();
		_generator.CreateSpawns();
		yield return StartCoroutine(WaitForElementDropped());
		yield return null;
	}

	private IEnumerator WaitForElementDropped()
	{
		yield return _generator.StartCoroutine(_generator.DropArena());
		yield return StartCoroutine(CountDown(3));
		_generator.StartCoroutine(_generator.DropArenaOverTime());
	}

	private IEnumerator CountDown(int startValue)
	{
		GameManager gm = GameManager.instance;

		_generator.CreatePlayers();
		gm.CountdownScreen.SetActive(true);

		int index = 0;
		while (index < 4)
		{
			Transform element = gm.CountdownScreen.transform.GetChild(index);
			element.gameObject.SetActive(true);
			float timer = 0;
			while(timer < 1)
			{
				timer += Time.deltaTime;
				Vector3 scale = Vector3.Slerp(Vector3.one * 0.5f, Vector3.one * 3, timer);
				element.localScale = scale;
				yield return null;
			}
			if(index == 3)
			{
				yield return new WaitForSeconds(1);
			}
			element.gameObject.SetActive(false);
			++index;
		}

		gm.CountdownScreen.SetActive(false);

		_generator.StartGame();
		_loaded = true;
	}

	public void ClearArena ()
	{
		_generator.EndStage();
	}

	private void OnPlayerWin(GameObject winner)
	{
		StopAllCoroutines();
		winner.GetComponent<Rigidbody>().isKinematic = true;
	}
}
