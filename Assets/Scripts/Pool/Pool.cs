using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct Pool
{
	private GameObjectPool _GOPInstance;

	public bool bIsOpen;
	public GameObject Prefab;
	public int Quantity;
	public int QuantityLoaded;
	public GameObject Root;
	public List<GameObject> Reserve;
	public string Name
	{
		get
		{
			return IsNull() ? "Unkown Pool" : Prefab.name;
		}
	}

	public void Drop()
	{
		GameObject.Destroy(Root);
	}

	public void Truncate()
	{
		Drop();
		Root = new GameObject(Name);
	}

	public void AddToPool (GameObject go)
	{
		Reserve.Add(go);
		go.transform.parent = Root.transform;
		go.transform.position = Vector3.zero;
		go.transform.localPosition = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		go.transform.localRotation = Quaternion.identity;
		go.gameObject.SetActive(false);
	}

	public void Init()
	{
		if (Prefab != null)
		{
			Poolable poolable = Prefab.GetComponent<Poolable>();
			if (poolable == null)
			{
				Prefab.AddComponent<Poolable>();
			}

			QuantityLoaded = 0;
			Root = new GameObject(Name);
			Root.transform.parent = GameObjectPool.Instance.transform;
			Reserve = new List<GameObject>();
		}
		else
		{
			Debug.LogError("Pool must have a prefab !!!");
			Debug.Break();
		}
	}

	public void SyncLoad(int quantityPerFrame)
	{
		for (int i = 0; i < quantityPerFrame; ++i)
		{
			GameObject go = GameObject.Instantiate(Prefab);
			go.transform.parent = Root.transform;
			go.gameObject.SetActive(false);
			go.name = Name + "_" + QuantityLoaded.ToString();
			go.GetComponent<Poolable>().Pool = this;

			go.transform.position = Vector3.zero;
			go.transform.localPosition = Vector3.zero;
			go.transform.rotation = Quaternion.identity;
			go.transform.localRotation = Quaternion.identity;

			Reserve.Add(go);

			++QuantityLoaded;
			++GameObjectPool.Instance.ElementsLoaded;
		}
	}

	public bool IsNull ()
	{
		return Prefab == null;
	}
}