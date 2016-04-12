using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	public static Arena instance;

	[Header("Arena Options")]
	private ArenaGenerator _generator;

	void Awake ()
	{
		instance = this;
	}

	// Use this for initialization
	public void StartGame()
	{
		_generator = GetComponent<ArenaGenerator>();

		// Start initialisation of arena field
		_generator.CreateArena();
		_generator.CreateSpawns();
		_generator.CreateObstacles();
		StartCoroutine(CountDown(3));
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
	}
}
