using UnityEngine;

public class Poolable : MonoBehaviour
{
	[HideInInspector]
	public Pool Pool;
	[HideInInspector]
    public bool IsInPool = false;
	
	public void AddToPool ()
    {
		MonoBehaviour[] tempScriptRefs = gameObject.GetComponentsInChildren<MonoBehaviour>();

		for (int i = 0; i < tempScriptRefs.Length; i++)
		{
			tempScriptRefs[i].StopAllCoroutines();
		}

        gameObject.transform.parent = Pool.Root.transform;
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
        IsInPool = true;
    }
}
