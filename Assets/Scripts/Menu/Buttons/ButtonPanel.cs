using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonPanel : MonoBehaviour
{
	public Transform OffsetTransf;
	public Button PreselectedButton;
	public float EndAlpha = 0.7f;
	public bool Preselected = false;
	private Vector3 _basePosition;

	private CanvasGroup _canvasGroup;
	private Button[] _buttonChildren;
	private bool[] _originalInteractables;
	private Button _lastSelectedButton;

	private bool _offseted = false;

	private Coroutine _activeMovementCoroutine;

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

		_offseted = !Preselected;

		if (Preselected)
			_canvasGroup.alpha = 1;
		else
			_canvasGroup.alpha = EndAlpha;
	}

	public void GotoOffset()
	{
		if (_offseted)
			return;

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_originalInteractables[i] = _buttonChildren[i].interactable;
			_buttonChildren[i].interactable = false;
		}

		if (_activeMovementCoroutine != null)
			StopCoroutine(_activeMovementCoroutine);
		_activeMovementCoroutine = StartCoroutine(MoveOverTime(_basePosition, OffsetTransf.localPosition, true));

		_offseted = true;
	}

	public void GotoBasePosition()
	{
		if (!_offseted)
			return;

		for (int i = 0; i < _buttonChildren.Length; i++)
		{
			_buttonChildren[i].interactable = _originalInteractables[i];
		}
		if (_activeMovementCoroutine != null)
			StopCoroutine(_activeMovementCoroutine);
		_activeMovementCoroutine = StartCoroutine(MoveOverTime(OffsetTransf.localPosition, _basePosition, false));

		if(PreselectedButton != null)
			PreselectedButton.Select();

		_offseted = false;
	}


	IEnumerator MoveOverTime(Vector3 start, Vector3 end, bool forward = false)
	{
		float eT = 0;
		float timeTaken = 0.3f;

		transform.localPosition = start;
		while (eT < timeTaken)
		{
			if(forward)
				_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, EndAlpha, eT / timeTaken);
			else
				_canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, 1, eT / timeTaken);

			eT += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(transform.localPosition, end, eT / timeTaken);
			yield return null;
		}
		transform.localPosition = end;
	}
}
