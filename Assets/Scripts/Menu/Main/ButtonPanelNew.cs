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
	public Selectable FirstElementSelected;

	private Selectable _lastSelectedElement;
	private Selectable _internalActiveButton;
	private Selectable ActiveElement
	{
		get { return _internalActiveButton; }
		set
		{
			if (value == null)
				_internalActiveButton = FirstElementSelected;
			else
				_internalActiveButton = value;
			_lastSelectedElement = _internalActiveButton;
			EventSystem.current.SetSelectedGameObject(null);
			EventSystem.current.SetSelectedGameObject(_internalActiveButton.gameObject);
		}
	}

	private MenuPanelNew _parentMenu;
	private Animator _animator;

	void Awake()
	{
		_parentMenu = GetComponentInParent<MenuPanelNew>();
		_animator = GetComponent<Animator>();
	}

	public void Open()
	{
		if (ParentButtonPanel != null)
			ParentButtonPanel.FadeParent();
		if (ActiveElement == null)
		{
			if (FirstElementSelected != null)
				ActiveElement = FirstElementSelected;
		}
		else
		{
			ActiveElement = ActiveElement;
		}

		_parentMenu.ActiveButtonPanel = this;
		MenuPanelNew.InputEnabled = false;
		LaunchAnimation("SendIn");
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
		LaunchAnimation("SendOut");
	}

	public void Fade()
	{
		LaunchAnimation("FadeOut");
	}

	public void FadeIn()
	{
		ActiveElement = _lastSelectedElement;
		_parentMenu.ActiveButtonPanel = this;
		MenuPanelNew.InputEnabled = false;
		_animator.ResetTrigger("FadeOut");
		LaunchAnimation("FadeIn");
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
		
		ActiveElement = newButton;
		_parentMenu.AddInputDelay();
	}

	public void Activate()
	{
		if(ActiveElement.GetComponent<Button>() != null)
			ActiveElement.GetComponent<Button>().onClick.Invoke();
	}

	public IEnumerator AnimCoroutine(string triggerName)
	{
		yield return new WaitUntil(() => { return _animator.isInitialized; });
		_animator.SetTrigger(triggerName);
	}

	public void LaunchAnimation(string triggerName)
	{
		StartCoroutine(AnimCoroutine(triggerName));
	}
}