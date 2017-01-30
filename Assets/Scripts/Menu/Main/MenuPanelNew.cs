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
	public static float GlobalInputDelay = 0;

	public string PanelName;
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
		PanelRefs.Add(PanelName, this);
		gameObject.SetActive(false);
		_animator = GetComponent<Animator>();
	}

	void Update()
	{
		if (ActiveMenupanel == null)
			return;
		if (ActiveMenupanel.GetInstanceID() != GetInstanceID())
			return;

		_activeInputDelay = _activeInputDelay.Reduce(Time.deltaTime);
		GlobalInputDelay = GlobalInputDelay.Reduce(Time.deltaTime);

		stickDirection = InputManager.GetAllStickDirection();
		if (stickDirection.magnitude < _previousStickDirection.magnitude - 0.3f)
		{
			_activeInputDelay = 0;
		}

		if (InputEnabled && GlobalInputDelay == 0)
			ProcessInput();
	}

	public virtual void Open()
	{
		if (!InputEnabled)
			return;

		gameObject.SetActive(true);

		if (ActiveMenupanel != null)
			ActiveMenupanel.Close();

		if (PreselectedButtonPanel != null)
		{
			ActiveButtonPanel = PreselectedButtonPanel;
			ActiveButtonPanel.Open();
		}

		ActiveMenupanel = this;
		InputEnabled = false;
		StartCoroutine(AnimCoroutine("SendIn"));
		SendOpen();
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

	public void SendOpen()
	{
		if (NetworkServer.active)
			Player.LocalPlayer.RpcMenuTransition(name, true);
	}

	public void FinishedEntering()
	{
		InputEnabled = true;
	}

	public void FinishedGoingOut()
	{
		gameObject.SetActive(false);
	}

	public virtual void Close()
	{
		LaunchAnimation("SendOut");
	}

	public void Return()
	{
		LaunchAnimation("SendOutRight");

		ActiveMenupanel = DefaultParentMenu;
		ActiveMenupanel.gameObject.SetActive(true);
		if(ActiveMenupanel.ActiveButtonPanel != null)
			ActiveMenupanel.ActiveButtonPanel.Open();

		ActiveMenupanel.LaunchAnimation("Return");
		InputEnabled = false;
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
