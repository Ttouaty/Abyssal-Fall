using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OptionButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
	void Awake()
	{
		OnDeselect(null);
	}

	public void OnSelect(BaseEventData eventData)
	{
		InputListener[] tempListener = GetComponents<InputListener>();
		for (int i = 0; i < tempListener.Length; i++)
		{
			tempListener[i].enabled = true;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		InputListener[] tempListener = GetComponents<InputListener>();
		for (int i = 0; i < tempListener.Length; i++)
		{
			tempListener[i].enabled = false;
		}
	}
}

