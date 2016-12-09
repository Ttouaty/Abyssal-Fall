using UnityEngine;
using UnityEngine.Networking;

/**
 * Most basic player possible. Use arrow keys to move around.
 */
public class NATTraversalExamplePlayer : NetworkBehaviour
{

    void Update()
    {
        if (!isLocalPlayer) return;

        Vector3 dir = Vector3.zero;

        if (Input.GetKey(KeyCode.UpArrow)) dir += Vector3.up;
        if (Input.GetKey(KeyCode.DownArrow)) dir += Vector3.down;
        if (Input.GetKey(KeyCode.LeftArrow)) dir += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow)) dir += Vector3.right;
        
        transform.position += dir * Time.deltaTime * 5;
    }
}
