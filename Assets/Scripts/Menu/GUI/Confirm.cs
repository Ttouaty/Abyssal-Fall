using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Confirm : MonoBehaviour
{
	public static Confirm Instance;

	[SerializeField]
	private Text _targetText;
	private string _defaultText;
	[SerializeField]
	private Text _yesText;
	private string _defaultYesText;
	[SerializeField]
	private Text _noText;
	private string _defaultNoText;

	private IEnumerator _yesCoroutine;
	private IEnumerator _noCoroutine;

	private Animator _animatorRef;

	private ReturnButton[] _returnButtonChildren;

	private EventSystem targetEventSystem;

	void Awake()
	{
		Instance = this;
		_animatorRef = GetComponent<Animator>();
		_defaultText = _targetText.text;
		_defaultYesText = _yesText.text;
		_defaultNoText = _noText.text;
		_returnButtonChildren = GetComponentsInChildren<ReturnButton>(true);

		for (int i = 0; i < _returnButtonChildren.Length; i++)
		{
			_returnButtonChildren[i].enabled = false;
		}
	}

	public void SetYesText(string newText) { _yesText.text = newText; }
	public void SetNoText(string newText) { _noText.text = newText; }

	public void Open(IEnumerator onYesCoroutine, IEnumerator onNoCoroutine) { Open(onYesCoroutine, onNoCoroutine, ""); }

	public void Open(IEnumerator onYesCoroutine, IEnumerator onNoCoroutine, string customText)
	{
		InputManager.AddInputLockTime(0.5f);
		_yesCoroutine = onYesCoroutine;
		_noCoroutine = onNoCoroutine;

		_targetText.text = customText.Length != 0 ? customText : _defaultText;
		_yesText.text = _defaultYesText;
		_noText.text = _defaultNoText;

		for (int i = 0; i < _returnButtonChildren.Length; i++)
		{
			_returnButtonChildren[i].enabled = true;
		}
		targetEventSystem = EventSystem.current;
		targetEventSystem.enabled = false;

		_animatorRef.SetTrigger("SendIn");
	}

	private void Close()
	{
		InputManager.AddInputLockTime(0.5f);
		_animatorRef.SetTrigger("SendOut");
		for (int i = 0; i < _returnButtonChildren.Length; i++)
		{
			_returnButtonChildren[i].enabled = false;
		}

		targetEventSystem.enabled = true;
	}

	public void OnYes()
	{
		if(_yesCoroutine != null)
			StartCoroutine(_yesCoroutine);
		Close();
	}

	public	void OnNo()
	{
		if(_noCoroutine != null)
			StartCoroutine(_noCoroutine);
		Close();
	}

}
