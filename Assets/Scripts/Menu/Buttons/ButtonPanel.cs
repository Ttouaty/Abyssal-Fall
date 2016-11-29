using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonPanel : MonoBehaviour
{
	public static ButtonPanel ActivePanel;

	public Transform OffsetTransf;
	public Button PreselectedButton;
	public float ClosedAlpha = 0.7f;
	public float InactiveAlpha = 0.7f;
	public bool Preselected = false;
	private Vector3 _basePosition;

	private CanvasGroup _canvasGroup;
	private Button[] _buttonChildren;
	private bool[] _originalInteractables;
	private Button _lastSelectedButton;

	private bool _isOffset = false;

	private Coroutine _activeMovementCoroutine;

	public ButtonPanel ParentPanel;
	private bool _active {
		get { return ActivePanel == this; }
	}

	private Button ButtonParent;
	private float targetAlpha = 1;

	private void Start()
	{
		_basePosition = transform.localPosition;
		_buttonChildren = GetComponentsInChildren<Button>();
		_originalInteractables = new bool[_buttonChildren.Length];
		_canvasGroup = GetComponent<CanvasGroup>();

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_originalInteractables[i] = _buttonChildren[i].interactable;
		}

		_isOffset = !Preselected;
		targetAlpha = _canvasGroup.alpha;

		if (OffsetTransf == null)
			OffsetTransf = transform;

		if (Preselected)
		{
			_isOffset = true; //hack
			Open(null);
		}
		else
		{
			_isOffset = false; //hack
			Close();
		}
	}

	void Update()
	{
		if (_active)
		{
			if (ParentPanel)
			{
				if (InputManager.GetButtonDown(InputEnum.B))
				{
					ParentPanel.Open(ParentPanel.ButtonParent);
					Close();
				}
			}
		}
		else if (!_isOffset)
		{
			targetAlpha = InactiveAlpha;
		}

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_buttonChildren[i].interactable = _active;
		}

		_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, 10 * Time.deltaTime);
	}

	public void Close()
	{
		targetAlpha = ClosedAlpha;
		if (_isOffset)
			return;

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_originalInteractables[i] = _buttonChildren[i].interactable;
			_buttonChildren[i].interactable = false;
		}

		if (_activeMovementCoroutine != null)
			StopCoroutine(_activeMovementCoroutine);
		_activeMovementCoroutine = StartCoroutine(MoveOverTime(_basePosition, OffsetTransf.localPosition, false));

		if (ButtonParent != null)
			ButtonParent.Select();

		_isOffset = true;

	}

	public void Open(Button newParent)
	{
		ActivePanel = this;
		targetAlpha = 1;
		if (!_isOffset)
			return;

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_buttonChildren[i].interactable = _originalInteractables[i];
		}
		if (_activeMovementCoroutine != null)
			StopCoroutine(_activeMovementCoroutine);
		_activeMovementCoroutine = StartCoroutine(MoveOverTime(OffsetTransf.localPosition, _basePosition, true));

		if(PreselectedButton != null)
			PreselectedButton.Select();

		ActivePanel = this;
		ButtonParent = newParent;
		_isOffset = false;
	}


	IEnumerator MoveOverTime(Vector3 start, Vector3 end, bool open = false)
	{
		float eT = 0;
		float timeTaken = 0.3f;

		transform.localPosition = start;
		while (eT < timeTaken)
		{
			//if(forward)
			//	_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, ClosedAlpha, eT / timeTaken);
			//else
			//	_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1, eT / timeTaken);
			

			eT += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(transform.localPosition, end, eT / timeTaken);
			yield return null;
		}
		transform.localPosition = end;
	}
}
