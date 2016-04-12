using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ArenaGenerator))]
public class Arena : MonoBehaviour
{
	private ArenaGenerator _generator;

	[Header("Arena Options")]
	[Tooltip("References to players meshes")]
	public GameObject[] PlayerRef;

	// Use this for initialization
	public void Init () {
		_generator = GetComponent<ArenaGenerator>();

		// Start initialisation of arena field
		_generator.CreateArena();
		_generator.CreateSpawns();
	}

	public void StartGame ()
	{
		StartCoroutine(CountDown(3));
	}

	private IEnumerator CountDown(int startValue)
	{
		GameManager gm = GameManager.instance;

		for (int s = 0; s < _generator.Spawns.Length; ++s)
		{
			if (_generator.Spawns[s] != null && PlayerRef[s] != null)
			{
				_generator.Spawns[s].SpawnPlayer(s, PlayerRef[s]);
			}
		}

		Text countDown = gm.CountdownScreen.transform.FindChild("CountDown").GetComponent<Text>();

		countDown.text = "Ready ?";
		yield return new WaitForSeconds(1);

		gm.CountdownScreen.SetActive(true);
		while (startValue > 0)
		{
			countDown.text = startValue.ToString();
			startValue--;
			yield return new WaitForSeconds(1);
		}

		countDown.text = "Go !!!";
		yield return new WaitForSeconds(1);

		gm.CountdownScreen.SetActive(false);

		for(int s = 0; s < _generator.Spawns.Length; ++s)
		{
			if (_generator.Spawns[s] != null)
			{
				_generator.Spawns[s].ActivatePlayer();
			}
		}
	}
}
