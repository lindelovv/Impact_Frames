using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogPopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textToDisplay;
    [SerializeField] private bool oneShot;
    [SerializeField] private bool randomDialogue;
    [SerializeField] private List<string> dialogueOptions = new List<string>();
    [SerializeField] GameObject objectToActivate;
    [SerializeField] private bool debug;
    private Keyboard keyboard;
    private bool hasBeenActivated = false;

    private System.Random random = new System.Random();

    private void Start()
    {
        objectToActivate.SetActive(false);

        if(debug)
        {
            keyboard = Keyboard.current;
        }
    }

    void Update()
    {
        if(debug && keyboard != null)
        {
            if(keyboard.oKey.wasPressedThisFrame)
            {
                Activate(); 
            }
            if(keyboard.pKey.wasPressedThisFrame)
            {
                DeActivate();
            }
        }
    }

    public void Activate()
    {
        if(oneShot && hasBeenActivated)
        {   
            return;
        }
        objectToActivate.SetActive(true);

        if(randomDialogue)
        {
            textToDisplay.text = dialogueOptions[random.Next(0, dialogueOptions.Count)];
        }
        else
        {
            textToDisplay.text = dialogueOptions[0];
        }
    }

    public void DeActivate()
    {
        hasBeenActivated = true;
        objectToActivate.SetActive(false);
    }
}
