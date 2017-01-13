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
	public LastSelectedComponent LastElementSelected;
	[SerializeField]
	private bool LastButtonReset = false;
	private LastSelectedComponent[] _selectables;
	private EventSystem _eventSystemRef;
	void Start()
	{
		_eventSystemRef = FindObjectOfType<EventSystem>();
		Selectable[] Tempselectables = GetComponentsInChildren<Selectable>(true);
		_selectables = new LastSelectedComponent[Tempselectables.Length];
		for (int i = 0; i < Tempselectables.Length; i++)
		{
			if (Tempselectables[i].GetComponent<LastSelectedComponent>() == null)
				_selectables[i] = Tempselectables[i].gameObject.AddComponent<LastSelectedComponent>();

			Tempselectables[i].gameObject.GetComponent<LastSelectedComponent>().ParentMenu = this;
		}
	}

	void LateUpdate()
	{
		if (LastElementSelected == null)
			return;

		if(!LastElementSelected.IsSelected)
		{
			if (FindSelected() != null)
			{
				LastElementSelected = FindSelected();
			}
			SelectLastButton();
		}
	}

	protected virtual void OnEnable()
	{
		if (LastElementSelected == null || LastButtonReset)
		{
			if (PreSelectedButton != null)
			{
				PreSelectedButton.Select();
				LastElementSelected = PreSelectedButton.GetComponent<LastSelectedComponent>();
			}
		}
		else
		{
			SelectLastButton();
		}
	}

	LastSelectedComponent FindSelected()
	{
		for (int i = 0; i < _selectables.Length; i++)
		{
			if (_selectables[i].IsSelected)
				return _selectables[i];
		}
		return null;
	}

	public void SelectLastButton()
	{
		_eventSystemRef.SetSelectedGameObject(null);
		_eventSystemRef.SetSelectedGameObject(LastElementSelected.gameObject);
	}

	public void ForceSelectedButton(GameObject target)
	{
		_eventSystemRef.SetSelectedGameObject(null);
		_eventSystemRef.SetSelectedGameObject(target);
	}
}
