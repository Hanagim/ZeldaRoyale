using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationManager : MonoBehaviour
{
    private Character _character;
    [SerializeField]
    private Button changeHatButton;
    [SerializeField]
    private NetworkManager networkManager;

    private void Start()
    {
        networkManager.OnClientConnectedCallback += CheckCharacters;
   
        changeHatButton.onClick.AddListener(ChangeHat);
    }

    private void CheckCharacters(ulong obj)
    {
        if(_character != null)
        {
            return;
        }
        var characters = FindObjectsOfType<Character>();
        _character = characters.Single(x => x.IsLocalPlayer);
    }

    void ChangeHat()
    {
        int newIndex = (_character.HatIndex.Value + 1) % 3;
        _character.HatIndex.Value = newIndex;
    }

        // Update is called once per frame
        void Update()
    {
        
    }
}
