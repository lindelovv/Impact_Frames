using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [SerializeField] private PlayerUI hostUI;
    [SerializeField] private PlayerUI connecterUI;
    [SerializeField] private float maxHealth;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void UpdateHealth(float currentHealth, int conncetionId)
    {
        if(conncetionId == 1)
        {
            hostUI.SetHealthBarFill(currentHealth / maxHealth);
        }
        else
        {
            connecterUI.SetHealthBarFill(currentHealth / maxHealth);
        }
    }

    public void DecreaseLife(int conncetionId)
    {
        if (conncetionId == 1)
        {
            hostUI.DecreaseLife();
        }
        else
        {
            connecterUI.DecreaseLife();
        }
    }

    public void UpdateComboMeter(int currentComboMeter, int conncetionId)
    {
        if (conncetionId == 1)
        {
            hostUI.SetComboFill(currentComboMeter);
        }
        else
        {
            connecterUI.SetComboFill(currentComboMeter);
        }
    }
}
