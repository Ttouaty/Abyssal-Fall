using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapSelectWheel : MenuWheel<WheelSelectable>
{
	private ArenaConfiguration_SO[] _configRefs;

	protected override void Start()
	{
		base.Start();
		DynamicConfig.Instance.GetConfigs(ref _configRefs);
		Generate();
	}

	public void Generate()
	{
		WheelSelectable[] elementsToProcess = new WheelSelectable[_configRefs.Length];

		GameObject tempGO;
		for (int i = 0; i < _configRefs.Length; i++)
		{
			tempGO = new GameObject();
			tempGO.AddComponent<RectTransform>();
			elementsToProcess[i] = tempGO.AddComponent<WheelSelectable>();
			elementsToProcess[i].sprite = _configRefs[i].Artwork;
		}

		base.Generate(elementsToProcess);

		for (int i = 0; i < _elementList.Count; i++)
		{
			_elementList[i].gameObject.name = "Arena_" + _configRefs[i].TargetMapEnum.ToString();
		}
	}
	
	public EArenaConfiguration GetSelectedElement()
	{
		return _configRefs[_selectedElementIndex].TargetMapEnum;
	}

	public void SendSelectionToGameManager()
	{
		GameManager.Instance.CurrentGameConfiguration.ArenaConfiguration = GetSelectedElement();
	}
}
