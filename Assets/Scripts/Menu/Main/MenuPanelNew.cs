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
	public static bool InputIsActive { get { return InputEnabled && !InputManager.InputLocked; } }
	public bool IsActive { get { return ActiveMenupanel == this; } }

	public string PanelName;
	[Space]
	public MenuPanelNew DefaultParentMenu;


	[HideInInspector]
	public ButtonPanelNew ActiveButtonPanel;
	public ButtonPanelNew PreselectedButtonPanel;
	private Animator _animator;
	private Vector2 _previousStickDirection;
	private Vector2 stickDirection;

	private float _defaultInputDelay = 0.1f;
	private float _repeatDelay = 0.5f;
	private float _activeInputDelay = 0;
	private string _lastTrigger = "";

	private bool _firstInput = true;

	void Awake()
	{
		if (PanelRefs.ContainsKey(PanelName))
			PanelRefs.Remove(PanelName);
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

		stickDirection = InputManager.GetAllStickDirection(false);
		if (stickDirection.magnitude < _previousStickDirection.magnitude - 0.3f)
		{
			_previousStickDirection = Vector3.zero;
			_activeInputDelay = 0;
			_firstInput = true;
		}

		if (InputEnabled && GlobalInputDelay == 0)
			ProcessInput();
	}


	public virtual void Open() { Open(true); }
	public virtual void Open(bool send)
	{
		if (!InputEnabled)
			return;

		gameObject.SetActive(true);

		if (ActiveMenupanel != null)
			if (!IsActive)
				ActiveMenupanel.Close();

		if (PreselectedButtonPanel != null)
		{
			ActiveButtonPanel = PreselectedButtonPanel;
			ActiveButtonPanel.Open();
		}

		ActiveMenupanel = this;
		InputEnabled = false;
		LaunchAnimation("SendIn");
		if(send)
			SendOpen();
	}

	public IEnumerator AnimCoroutine(string triggerName)
	{
		yield return new WaitUntil(() => { return _animator.isInitialized; });
		if (_lastTrigger.Length != 0)
			_animator.ResetTrigger(_lastTrigger);

		_lastTrigger = triggerName;

		_animator.SetTrigger(triggerName);
	}

	public void LaunchAnimation(string triggerName)
	{
		if (isActiveAndEnabled)
			StartCoroutine(AnimCoroutine(triggerName));
	}

	public void SendOpen()
	{
		if (NetworkServer.active)
			Player.LocalPlayer.RpcMenuTransition(PanelName, true);
	}

	public virtual void FinishedEntering()
	{
		InputEnabled = true;
	}

	public virtual void FinishedGoingOut()
	{
		gameObject.SetActive(false);
	}

	public virtual void Close()
	{
		LaunchAnimation("SendOut");
	}

	public virtual void Return()
	{
		LaunchAnimation("SendOutRight");

		ActiveMenupanel = DefaultParentMenu;
		ActiveMenupanel.gameObject.SetActive(true);
		if(ActiveMenupanel.ActiveButtonPanel != null)
			ActiveMenupanel.ActiveButtonPanel.FadeIn();
		else
		{
			ActiveMenupanel.ActiveButtonPanel = ActiveMenupanel.PreselectedButtonPanel;
			if(ActiveMenupanel.ActiveButtonPanel != null)
				ActiveMenupanel.ActiveButtonPanel.Open();
		}

		ActiveMenupanel.LaunchAnimation("Return");
		InputEnabled = false;
		if (NetworkServer.active)
			Player.LocalPlayer.RpcMenuTransition(PanelName, false);
	}

	void ProcessInput()
	{
		if(ActiveButtonPanel != null)
		{
			if (InputManager.GetButtonDown(InputEnum.A))
			{
				_previousStickDirection = stickDirection;
				ActiveButtonPanel.Activate();
			}

			if (InputManager.GetButtonDown(InputEnum.B))
			{
				_previousStickDirection = stickDirection;
				ActiveButtonPanel.Return();
			}

			if (_activeInputDelay != 0)
				return;

			if(stickDirection.magnitude > 0.8f)
			{
					if (stickDirection.AnglePercent(Vector2.up) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Up);
				else if (stickDirection.AnglePercent(Vector2.down) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Down);
				else if (stickDirection.AnglePercent(Vector2.left) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Left);
				else if (stickDirection.AnglePercent(Vector2.right) > 0.5f)
					ActiveButtonPanel.SelectNewButton(UIDirection.Right);

				if(_firstInput)
				{
					_activeInputDelay = _repeatDelay;
					_firstInput = false;
				}

				_previousStickDirection = stickDirection;
			}
		}
	}

	public void AddInputDelay(float addDelay = -1)
	{
		if (addDelay == -1)
			addDelay = _defaultInputDelay;

		_activeInputDelay += addDelay;

	}

	public void SetGlobalDelay(float delay)
	{
		GlobalInputDelay = delay;
	}
}
