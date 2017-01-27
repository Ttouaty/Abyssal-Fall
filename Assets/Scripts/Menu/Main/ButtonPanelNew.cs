using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum UIDirection
{
	Up, Down, Left, Right
}

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class ButtonPanelNew : MonoBehaviour
{
	public ButtonPanelNew ParentButtonPanel;
	public Button FirstElementSelected;

	private Button _lastSelectedElement;
	private Button _internalActiveButton;
	private Button ActiveElement
	{
		get { return _internalActiveButton; }
		set
		{
			if (value == null)
				_internalActiveButton = FirstElementSelected;
			else
				_internalActiveButton = value;
			_lastSelectedElement = _internalActiveButton;
			_internalActiveButton.Select();
		}
	}

	private MenuPanelNew _parentMenu;
	private Animator _animator;

	void Start()
	{
		_animator = GetComponent<Animator>();
		_parentMenu = GetComponentInParent<MenuPanelNew>();

	}

	public void Open()
	{
		if (ParentButtonPanel != null)
			ParentButtonPanel.FadeParent();
		if (FirstElementSelected != null)
			ActiveElement = FirstElementSelected;

		_parentMenu.ActiveButtonPanel = this;
		MenuPanelNew.InputEnabled = false;
		_animator.SetTrigger("SendIn");
	}

	private void FadeParent()
	{
		if (ParentButtonPanel != null)
			ParentButtonPanel.FadeParent();

		Fade();
	}

	public void FinishedEntering()
	{
		MenuPanelNew.InputEnabled = true;
	}

	public void Close()
	{
		_animator.SetTrigger("SendOut");
	}

	public void Fade()
	{
		_animator.SetTrigger("FadeOut");
	}

	public void FadeIn()
	{
		ActiveElement = _lastSelectedElement;
		_parentMenu.ActiveButtonPanel = this;
		MenuPanelNew.InputEnabled = false;
		_animator.SetTrigger("FadeIn");
	}

	public void Return()
	{
		if (ParentButtonPanel == null)
			return;

		ParentButtonPanel.FadeIn();
		Close();
	}

	public void SelectNewButton(UIDirection newDirection)
	{
		if (ActiveElement == null)
			return;

		if (newDirection == UIDirection.Up)
			SelectNewButton(ActiveElement.navigation.selectOnUp);
		else if (newDirection == UIDirection.Down)
			SelectNewButton(ActiveElement.navigation.selectOnDown);
		else if (newDirection == UIDirection.Left)
			SelectNewButton(ActiveElement.navigation.selectOnLeft);
		else
			SelectNewButton(ActiveElement.navigation.selectOnRight);
	}

	public void SelectNewButton(Selectable newButton)
	{
		if (newButton == null)
			return;
		
		ActiveElement = (Button)newButton;
		_parentMenu.AddInputDelay();
	}

	public void Activate()
	{
		ActiveElement.onClick.Invoke();
	}
}