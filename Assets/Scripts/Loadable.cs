using UnityEngine;
using UnityEngine.Events;
using System.Collections;

[System.Serializable]
public class LoadableMessageEvent : UnityEvent<string> { }
[System.Serializable]
public class LoadableEndEvent : UnityEvent { }

public class Loadable : MonoBehaviour
{
	public LoadableMessageEvent OnMessage;
	public LoadableEndEvent OnLoadComplete;

	virtual public void Init()
	{

	}
}
