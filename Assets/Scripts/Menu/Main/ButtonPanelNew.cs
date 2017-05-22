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
	private static EventSystem _TargetEventSystem;
	public ButtonPanelNew ParentButtonPanel;
	public Selectable FirstElementSelected;

	private Selectable _lastSelectedElement;
	protected Selectable _internalActiveButton;
	protected Selectable ActiveElement
	{
		get { return _internalActiveButton; }
		set
		{
			if (value == null)
				_internalActiveButton = FirstElementSelected;
			else
				_internalActiveButton = value;

			_lastSelectedElement = _internalActiveButton;

			if (_TargetEventSystem == null)
				_TargetEventSystem = EventSystem.current;

			if (_TargetEventSystem != null)
			{
				_TargetEventSystem.SetSelectedGameObject(null);
				_TargetEventSystem.SetSelectedGameObject(_internalActiveButton.gameObject);
			}
		}
	}

	protected MenuPanelNew _parentMenu;
	private Animator _animator;
	private string _lastTrigger = "";

	void Awake()
	{
		_parentMenu = GetComponentInParent<MenuPanelNew>();
		_animator = GetComponent<Animator>();
	}

	public virtual void Open()
	{
		if (ParentButtonPanel != null)
			ParentButtonPanel.FadeParent();
		ActiveElement = FirstElementSelected;

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

	public void FadeIn() // duplicate but fuck it :p
	{
		if (ParentButtonPanel != null)
			ParentButtonPanel.FadeParent();
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

		SoundManager.Instance.PlayOS("UI button Cancel");

		ParentButtonPanel.Invoke("FadeIn",0.05f);
		_parentMenu.SetInputDelay();
		Close();
	}

	public virtual void SelectNewButton(UIDirection newDirection)
	{
		if (ActiveElement == null)
			return;

		SelectNewButton(GetNextButton(newDirection, ActiveElement));
	}

	public Selectable GetNextButton(UIDirection newDirection, Selectable targetSelectable, Selectable firstSelected = null)
	{
		if (targetSelectable.navigation.mode == Navigation.Mode.None)
			return null;

		if (firstSelected == targetSelectable)
		{
			Debug.Log("Full loop, cancelling !");
			return null;
		}

		if (firstSelected == null)
			firstSelected = targetSelectable;

		Selectable newTargetSelectable;

		if (newDirection == UIDirection.Up)
			newTargetSelectable = targetSelectable.navigation.selectOnUp;
		else if (newDirection == UIDirection.Down)
			newTargetSelectable = targetSelectable.navigation.selectOnDown;
		else if (newDirection == UIDirection.Left)
			newTargetSelectable = targetSelectable.navigation.selectOnLeft;
		else
			newTargetSelectable = targetSelectable.navigation.selectOnRight;

		if (newTargetSelectable == null)
			return null;

		if (newTargetSelectable.interactable && newTargetSelectable.isActiveAndEnabled)
			return newTargetSelectable;
		return GetNextButton(newDirection, newTargetSelectable, firstSelected);
	}

	public void SelectNewButton(Selectable newButton)
	{
		if (newButton == null)
			return;
		
		ActiveElement = newButton;
		SoundManager.Instance.PlayOS("UI button Change 1");
		_parentMenu.SetInputDelay();
	}

	public virtual void Activate()
	{
		if(ActiveElement.isActiveAndEnabled)
			if(ActiveElement.GetComponent<Button>() != null)
			{
				SoundManager.Instance.PlayOS("UI button Select 2");
				ActiveElement.GetComponent<Button>().onClick.Invoke();
				_parentMenu.SetInputDelay();
			}
	}

	public IEnumerator AnimCoroutine(string triggerName)
	{
		yield return new WaitUntil(() => { return _animator.isInitialized; });
		_animator.SetTrigger(triggerName);
	}

	public void LaunchAnimation(string triggerName)
	{
		if (_lastTrigger.Length != 0)
			_animator.ResetTrigger(_lastTrigger);

		_lastTrigger = triggerName;

		StartCoroutine(AnimCoroutine(triggerName));
	}
}