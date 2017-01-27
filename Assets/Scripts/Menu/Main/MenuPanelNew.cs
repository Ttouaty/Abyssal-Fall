using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

[RequireComponent(typeof(Animator), typeof(CanvasGroup))]
public class MenuPanelNew : MonoBehaviour
{
	public static Dictionary<string, MenuPanelNew> PanelRefs = new Dictionary<string, MenuPanelNew>();
	public static MenuPanelNew ActiveMenupanel;
	public static bool InputEnabled = true;

	[Space]
	public MenuPanelNew DefaultParentMenu;


	[HideInInspector]
	public ButtonPanelNew ActiveButtonPanel;
	public ButtonPanelNew PreselectedButtonPanel;
	private Animator _animator;
	private Vector2 _previousStickDirection;
	private Vector2 stickDirection;

	private float _defaultInputDelay = 0.2f;
	private float _activeInputDelay = 0;

	void Awake()
	{
		PanelRefs.Add(name, this);
	}

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

	public virtual void Open()
	{
		if (!InputEnabled)
			return;

		if (ActiveMenupanel != null)
			ActiveMenupanel.Close();

		if (PreselectedButtonPanel != null)
		{
			ActiveButtonPanel = PreselectedButtonPanel;
			ActiveButtonPanel.Open();
		}

		ActiveMenupanel = this;
		InputEnabled = false;
		_animator.SetTrigger("SendIn");
		SendOpen();
	}

	public void SendOpen()
	{
		if (NetworkServer.active)
			Player.LocalPlayer.RpcMenuTransition(name, true);
	}

	public void FinishedEntering()
	{
		InputEnabled = true;
	}

	public virtual void Close()
	{
		_animator.SetTrigger("SendOut");
	}

	public void Return()
	{
		Close();
		ActiveMenupanel = DefaultParentMenu;
		if(ActiveButtonPanel != null)
			ActiveButtonPanel.Open();

		InputEnabled = false;
		_animator.SetTrigger("Return");
		if (NetworkServer.active)
			Player.LocalPlayer.RpcMenuTransition(DefaultParentMenu.name, false);
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
