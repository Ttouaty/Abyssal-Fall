using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(Button))]
public class LastSelectedComponent : MonoBehaviour, ISelectHandler{

	public MenuPanel ParentMenu;

	public void OnSelect(BaseEventData eventData)
	{
		ParentMenu.LastButtonSelected = GetComponent<Button>();
	}
}
