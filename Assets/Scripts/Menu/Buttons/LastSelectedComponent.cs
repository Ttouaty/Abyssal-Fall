using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

[RequireComponent(typeof(Selectable))]
public class LastSelectedComponent : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler{

	public MenuPanel ParentMenu;
	private Selectable _selectRef;
	public bool IsSelected = false;
	void Awake()
	{
		_selectRef = GetComponent<Selectable>();
		if (GetComponentInChildren<Graphic>())
			GetComponentInChildren<Graphic>().raycastTarget = true;
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnSelectHandler();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnDeselectHandler();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnSelectHandler();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnDeselectHandler();
	}

	void OnSelectHandler()
	{
		if (!_selectRef.interactable || ParentMenu.LastElementSelected == _selectRef)
			return;
		IsSelected = true;
		ParentMenu.LastElementSelected = this;	
		_selectRef.Select();
	}

	void OnDeselectHandler()
	{
		if (!_selectRef.interactable)
			return;
		IsSelected = false;
	}

	public void Select()
	{
		_selectRef.Select();
	}
}
