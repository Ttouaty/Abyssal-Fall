using UnityEngine;

public class Poolable : MonoBehaviour
{
	public Pool Pool;
    public bool IsInPool = false;
	
	public void AddToPool ()
    {
        Pool.Reserve.Add(gameObject);
        gameObject.transform.parent = Pool.Root.transform;
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.gameObject.SetActive(false);
        IsInPool = true;
    }
}
