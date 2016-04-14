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

	void Update ()
	{
		if(_loaded)
		{
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				StartCoroutine(StartGame());
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				Time.timeScale = Time.timeScale == 0 ? 1 : 0;
			}
		}
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

		Text countDown = gm.CountdownScreen.transform.FindChild("CountDown").GetComponent<Text>();

		gm.CountdownScreen.SetActive(true);
		countDown.text = "Ready ?";
		yield return new WaitForSeconds(1);

		while (startValue > 0)
		{
			countDown.text = startValue.ToString();
			startValue--;
			yield return new WaitForSeconds(1);
		}

		countDown.text = "Go !!!";
		yield return new WaitForSeconds(1);

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
