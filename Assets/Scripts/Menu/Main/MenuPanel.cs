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
	public Selectable LastElementSelected;

	private Selectable[] _selectables;

	[HideInInspector]
	public bool NeedReselect = false;
	void Start()
	{
		_selectables = GetComponentsInChildren<Selectable>();
		for (int i = 0; i < _selectables.Length; i++)
		{
			if (_selectables[i].GetComponent<LastSelectedComponent>() == null)
				_selectables[i].gameObject.AddComponent<LastSelectedComponent>();

			_selectables[i].gameObject.GetComponent<LastSelectedComponent>().ParentMenu = this;
		}
	}

	void Update()
	{
		if (LastElementSelected == null)
			return;

		if (InputManager.AnyDown() && NeedReselect)
			LastElementSelected.Select();
	}
}
