using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WheelSelectable : Image
{
	public virtual void Generate(Transform newParent, Vector3 position, Vector2 newSizeDelta)
	{
		transform.SetParent(newParent);
		transform.position = position;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		
		preserveAspect = true;

		rectTransform.sizeDelta = newSizeDelta;
		color = new Color(color.r, color.g, color.b, 0);
	}
}