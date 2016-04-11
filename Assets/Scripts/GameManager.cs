using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
	private Text _loadingType;

	public DynamicFieldGenerator DynamicFieldGenerator;
	public GameObject LoadingScreen;

	// Use this for initialization
	void Start ()
	{
		/* Check if the required members are set */
		if (DynamicFieldGenerator == null)
		{
			Debug.LogError("The needed reference to the DynamicFieldGenerator is missing. Please correct this and restart !");
			Debug.Break();
		}

		if (LoadingScreen == null)
		{
			Debug.LogError("The needed reference to the LoadingScreen is missing. Please correct this and restart !");
			Debug.Break();
		}

		_loadingType = LoadingScreen.transform.FindChild("LoadingType").GetComponent<Text>();

		LoadingScreen.SetActive(true);

		/* Add events */
		GameObjectPool.instance.OnStartLoad.AddListener(OnChangeLoadingMessage);
		GameObjectPool.instance.OnLoadComplete.AddListener(OnGameObjectPoolLoaded);

		DynamicFieldGenerator.OnStartLoad.AddListener(OnChangeLoadingMessage);
		DynamicFieldGenerator.OnLoadComplete.AddListener(OnDynamicFieldGeneratorLoaded);
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnChangeLoadingMessage(string msg)
	{
		_loadingType.text = msg;
	}

	void OnGameObjectPoolLoaded(string msg)
	{
		DynamicFieldGenerator.Init();
	}

	void OnDynamicFieldGeneratorLoaded(string msg)
	{
		_loadingType.text = "";
		LoadingScreen.SetActive(false);
	}
}
