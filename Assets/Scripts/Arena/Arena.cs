using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	public static Arena instance;

	private ArenaGenerator _generator;
	private bool _loaded;

	void Awake ()
	{
		instance = this;
		_loaded = false;
	}

	void Update ()
	{
		if(_loaded)
		{
			if(Input.GetKeyDown(KeyCode.Escape)) {
				_generator.ResetGame();
				StartGame();
			}
		}
	}

	// Use this for initialization
	public void StartGame()
	{
		_generator = GetComponent<ArenaGenerator>();

		// Start initialisation of arena field
		_generator.CreateArena();
		_generator.CreateSpawns();
		StartCoroutine(WaitForElementDropped());
	}

	private IEnumerator WaitForElementDropped()
	{
		yield return _generator.StartCoroutine(_generator.DropArena());
		yield return StartCoroutine(CountDown(3));
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
}
