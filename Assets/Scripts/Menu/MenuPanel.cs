using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour {
	public string MenuName;
	public Button PreSelectedButton;
	[HideInInspector]
	public MenuPanel ParentMenu;
	
	void Start()
	{
		ParentMenu = GetComponentInParent<MenuPanel>();
	}
}
