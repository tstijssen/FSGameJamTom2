using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.Discovery
{
    public class ServerButtonBehaviour : MonoBehaviour
    {
        public ServerResponse serverInfo;

        public void Connect()
        {
            NetworkManager.singleton.StartClient(serverInfo.uri);
        }

        public void Refresh()
        {
            NetworkManager.singleton.GetComponent<CustomNetworkDiscoveryHud>().RefreshServers();
        }

        public void Host()
        {
            NetworkManager.singleton.GetComponent<CustomNetworkDiscoveryHud>().Host();
        }
    }
}
