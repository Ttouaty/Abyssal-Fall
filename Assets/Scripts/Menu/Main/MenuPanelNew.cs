using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class MenuPanelNew : MonoBehaviour
{
	private static MenuPanelNew ActiveMenupanel;
	public static bool InputEnabled = true;

	[Space]
	public MenuPanel DefaultParentMenu;
	private MenuPanel _parentMenu;

	[HideInInspector]
	public ButtonPanelNew ActiveButtonPanel;

	private Animator _animator;
	private Vector2 _previousStickDirection;
	private Vector2 stickDirection;

	private float _defaultInputDelay = 0.2f;
	private float _activeInputDelay = 0;

	void Start()
	{
		_animator = GetComponent<Animator>();
	}

	void Update()
	{
		if (ActiveMenupanel == null)
			return;
		if (ActiveMenupanel.GetInstanceID() != GetInstanceID())
			return;

		_activeInputDelay = _activeInputDelay.Reduce(Time.deltaTime);

		stickDirection = InputManager.GetAllStickDirection();
		if (stickDirection.magnitude < _previousStickDirection.magnitude - 0.5f)
			_activeInputDelay = 0;

		if (InputEnabled)
			ProcessInput();
	}

	public void Open()
	{
		if (ActiveMenupanel != null)
			ActiveMenupanel.Close();

		ActiveMenupanel = this;
		InputEnabled = false;
		_animator.SetTrigger("SendIn");
	}

	public void FinishedEntering()
	{
		InputEnabled = true;
	}

	public void Close()
	{
		_animator.SetTrigger("SendOut");
	}


	void ProcessInput()
	{
		if(ActiveButtonPanel != null)
		{
			if (InputManager.GetButtonDown(InputEnum.A))
			{
				ActiveButtonPanel.Activate();
			}
			
			if (InputManager.GetButtonDown(InputEnum.B))
			{
				ActiveButtonPanel.Return();
			}

			if (_activeInputDelay != 0)
				return;

			if(stickDirection.magnitude > 0.9f)
			{
				if (stickDirection.AnglePercent(Vector2.up) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Up);
				else if (stickDirection.AnglePercent(Vector2.down) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Down);
				else if (stickDirection.AnglePercent(Vector2.left) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Left);
				else if (stickDirection.AnglePercent(Vector2.right) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Right);
			}
		}
	}

	public void AddInputDelay(float addDelay = -1)
	{
		if (addDelay == -1)
			addDelay = _defaultInputDelay;

		_activeInputDelay += addDelay;

	}
}
