using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class DynamicFieldGeneratorEvent : UnityEvent<string> { }

public class DynamicFieldGenerator : MonoBehaviour
{
	private GameObject[] _tiles;

	public uint Size = 20;
	public string PoolName;

	public GameObjectPoolEvent OnStartLoad;
	public GameObjectPoolEvent OnLoadComplete;

	// Use this for initialization
	public void Init () {
		if (!GameObjectPool.PoolExists(PoolName))
		{
			Debug.LogError("The pool " + PoolName + " don't exists. Please verify if it exists or create it");
			Debug.Break();
		}
		else
		{
			OnStartLoad.Invoke("Building Arena");
			StartCoroutine(LoadRow());
		}
	}

	private IEnumerator LoadRow ()
	{
		_tiles = new GameObject[Size * Size];
		for (var z = 0; z < Size; ++z)
		{
			for (var x = 0; x < Size; ++x)
			{
				/* TODO : Run the algorythm of generation */
				GameObject tile = GameObjectPool.GetAvailableObject("Tile.Default");
				tile.transform.position = new Vector3(-Size * 0.5f + x, 0, -Size * 0.5f + z);
				tile.transform.parent = transform;
			}
			yield return null;
		}
	}
}
