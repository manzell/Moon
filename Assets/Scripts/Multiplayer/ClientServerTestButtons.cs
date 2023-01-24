using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode; 

namespace moon
{
    public class ClientServerTestButtons : MonoBehaviour
    {
        [SerializeField] Button ServerButton, HostButton, ClientButton;

        private void Awake()
        {
            ServerButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
            HostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
            ClientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        }
    }
}
