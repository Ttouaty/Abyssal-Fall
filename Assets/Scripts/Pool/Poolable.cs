using UnityEngine;
using System.Collections;

public class Poolable : MonoBehaviour
{
	private bool _canBePooled = true;

	[HideInInspector]
	public string PoolName;

	public void SetReturnToPool(bool value)
	{
		_canBePooled = value;
	}

	void OnBecameInvisible ()
	{
		if(_canBePooled)
		{
			GameObjectPool.AddObjectIntoPool(gameObject);
		}
	}
}
