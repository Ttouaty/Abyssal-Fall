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
		if(transform.parent.GetComponent<MenuPanel>() != null)
			ParentMenu = transform.parent.GetComponent<MenuPanel>();
	}
}
