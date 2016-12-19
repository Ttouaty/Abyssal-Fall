using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AutoLinkToHost : MonoBehaviour
{
	public MapSelectWheel Target;
	void Update()
	{
		if(NetworkServer.active)
		{
			if(ServerManager.Instance.HostingClient != null)
			{
				if (Target.ParentPlayer == null)
					Target.GetComponent<NetworkIdentity>().AssignClientAuthority(ServerManager.Instance.HostingClient.connectionToClient);
				Target.ParentPlayer = ServerManager.Instance.HostingClient;
			}
		}
		else
			Target.ParentPlayer = null;
	}
}
