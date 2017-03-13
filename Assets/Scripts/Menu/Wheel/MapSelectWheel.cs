using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class MapSelectWheel : MenuWheel<EArenaConfiguration>
{
	private ArenaConfiguration_SO[] _configRefs;

	protected override void Start()
	{
		base.Start();
		transform.SetParent(MenuManager.Instance.GetComponentInChildren<MapSelector>(true).MapWheelTarget, false); //Moche mais j'ai la flemme nigga ! osef des perfs dans le menu
		DynamicConfig.Instance.GetConfigs(ref _configRefs);
		Generate();
	}

	public void Generate()
	{
		GameObject[] elementsToProcess = new GameObject[_configRefs.Length];
		EArenaConfiguration[] returnArray = new EArenaConfiguration[_configRefs.Length];
		_selectedElementIndex = 0;

		Image tempImageRef;
		_wheelRadius = Mathf.Abs(transform.parent.localPosition.z);
		for (int i = 0; i < _configRefs.Length; i++)
		{
			elementsToProcess[i] = new GameObject("Arena_" + _configRefs[i].TargetMapEnum.ToString(), typeof(Image));
			tempImageRef = elementsToProcess[i].GetComponent<Image>();
			tempImageRef.sprite = _configRefs[i].Artwork;

			tempImageRef.preserveAspect = true;

			tempImageRef.color = new Color(tempImageRef.color.r, tempImageRef.color.g, tempImageRef.color.b, 0);

			elementsToProcess[i].transform.SetParent(transform);
			elementsToProcess[i].transform.localScale = Vector3.one;
			tempImageRef.rectTransform.sizeDelta = transform.parent.GetComponent<RectTransform>().sizeDelta;

			returnArray[i] = _configRefs[i].TargetMapEnum;
		}

		base.Generate(elementsToProcess, returnArray);
	}

	protected override void Update()
	{
		base.Update();
		for (int i = 0; i < _displayArray.Length; i++)
		{
			if (Vector3.Angle(-Camera.main.transform.forward.ZeroY().normalized, (_displayArray[i].transform.position - transform.position).ZeroY().normalized) < _rotationBetweenElements * 0.75f)
				_displayArray[i].transform.SetAsLastSibling();
		}
	}

	public void SendSelectionToGameManager()
	{
		GameManager.Instance.CurrentGameConfiguration.ArenaConfiguration = GetSelectedElement();
	}

	public override void ScrollToIndex(int newIndex)
	{
		if(NetworkServer.active)
			base.ScrollToIndex(newIndex);
	}
}
