using UnityEngine;

public class Poolable : MonoBehaviour
{
	[HideInInspector]
	public Pool Pool;
	[HideInInspector]
    public bool IsInPool = false;
	
	public void AddToPool ()
    {
		MonoBehaviour[] tempScriptRefs = gameObject.GetComponentsInChildren<MonoBehaviour>(true);

		for (int i = 0; i < tempScriptRefs.Length; i++)
		{
			tempScriptRefs[i].StopAllCoroutines();
		}

		if (Pool.IsNull())
			return;
		
        gameObject.transform.parent = Pool.Root.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.SetActive(false);
        IsInPool = true;
    }
}
