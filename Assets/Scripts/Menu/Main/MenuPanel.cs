using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuPanel : MonoBehaviour {
	public string MenuName;
	public Button PreSelectedButton;
	[Space]
	public MenuPanel ParentMenu;
	[HideInInspector]
	public Button LastButtonSelected; // NOT YET IMPLEMENTED

	void Start()
	{
		Button[] tempButtonArray = GetComponentsInChildren<Button>();
		for (int i = 0; i < tempButtonArray.Length; i++)
		{
			if (tempButtonArray[i].GetComponent<LastSelectedComponent>() == null)
				tempButtonArray[i].gameObject.AddComponent<LastSelectedComponent>();

			tempButtonArray[i].GetComponent<LastSelectedComponent>().ParentMenu = this;
		}
	}
}
