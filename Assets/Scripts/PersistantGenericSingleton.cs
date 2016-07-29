using UnityEngine;
using System.Collections;

public abstract class PersistantGenericSingleton<T> : GenericSingleton<T> where T : Component
{
	public override void OnLevelWasLoaded()
	{
		DontDestroyOnLoad(this.gameObject);
		base.OnLevelWasLoaded();
	}
}
