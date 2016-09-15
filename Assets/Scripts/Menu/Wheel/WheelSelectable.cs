using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WheelSelectable : Image
{
	public virtual void Generate(Transform newParent, Vector3 position, Vector2 newSizeDelta)
	{
		transform.SetParent(newParent);

		preserveAspect = true;

		rectTransform.localScale = Vector3.one;
		rectTransform.sizeDelta = newSizeDelta;
		rectTransform.localRotation = Quaternion.identity;
		rectTransform.position = position;
		color = new Color(color.r, color.g, color.b, 0);
	}
}