using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private RectTransform comboBarFill;
    [SerializeField] private Slider healthBarFill;
    [SerializeField] private GameObject[] lives;
    private float comboBarFillWidth;

    public void SetComboFill(int comboMeterAmount)
    {
        comboBarFill.offsetMax = new Vector2( -(comboBarFillWidth- (comboBarFillWidth/100) * comboMeterAmount), 0);
    }

    public void SetHealthBarFill(float healthAmount)
    {
        healthBarFill.value = healthAmount;
    }

    public void DecreaseLife()
    {
        for(int i = 0; i < lives.Length; i++)
        {
            if (lives[i].activeInHierarchy)
            {
                lives[i].SetActive(false);
                return;
            }
        }
    }

    //void Start()
    //{
    //   comboBarFillWidth = GetComponent<RectTransform>().rect.width;
    //   SetComboFill(0);
    //   SetHealthBarFill(0.5f);
    //
    //   DecreaseLife();
    //}

    //void Update()
    //{
    //      GetComponent<RectTransform>().rect.Set(GetComponent<RectTransform>().rect.x , GetComponent<RectTransform>().rect.y, GetComponent<RectTransform>().rect.width + 1, GetComponent<RectTransform>().rect.height + 1);
    //      GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,800);
    //}
}
