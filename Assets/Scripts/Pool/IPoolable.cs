using UnityEngine;
using System.Collections;

public interface IPoolable
{
    void OnGetFromPool();
    void OnReturnToPool();
}
