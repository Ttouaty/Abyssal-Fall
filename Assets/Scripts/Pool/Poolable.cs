using UnityEngine;

public class Poolable : MonoBehaviour
{
	[HideInInspector]
	public Pool Pool;
    public bool IsInPool = false;
	
	public void AddToPool ()
    {
        gameObject.transform.parent = Pool.Root.transform;
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.SetActive(false);
        IsInPool = true;
    }
}
