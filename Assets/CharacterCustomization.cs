using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public
    class CharacterCustomization : MonoBehaviour
{
    public List<GameObject> Hats = new();
    public Character _character;

    // Start is called before the first frame update
    void Start()
    {
        _character.HatIndex.OnValueChanged += ChangeHatIndex;
        ChangeHatIndex(0, _character.HatIndex.Value);
    }

    private void ChangeHatIndex(int previousValue, int newValue)
    {
        for (int i = 0; i < Hats.Count; i++)
        {
            GameObject hat = Hats[i];
            hat.SetActive(false);
            
        }
        Hats[newValue].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
