using UnityEngine;
using System.Collections;

public abstract class ABehavior : MonoBehaviour
{
    abstract public void Start();
    abstract public void Update();
    abstract public void End();

    abstract public void SetActive(bool bActivate);
}
