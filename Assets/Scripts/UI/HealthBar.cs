using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Character character;
    private int playerHealth;
    [SerializeField]
    private Image healthBarImage; 

    // Start is called before the first frame update
    void Start()
    {
        character.PlayerHealth.OnValueChanged += ChangeFillAmount;
    }

    private void ChangeFillAmount(int previousValue, int newValue)
    {
        healthBarImage.fillAmount = newValue / (float)character.MaxHealth;
    }
}
