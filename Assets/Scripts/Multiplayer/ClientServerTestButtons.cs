using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode; 

namespace moon
{
    public class ClientServerTestButtons : MonoBehaviour
    {
        [SerializeField] Button HostButton, ClientButton;

        private void Awake()
        {
            HostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
            ClientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        }
    }
}
