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
    public TMP_FontAsset fontAsset;
    private Keyboard keyboard;
    private bool hasBeenActivated = false;

    private System.Random random = new System.Random();


    /**
     * Make Sure to slot a font asset in the inspector, 
     * If noone exist, use Windows, TextMeshPro, FontAssetCreator togGenerate Font Atlas
     * The text properties like color and glow is set on the Font Atlas material in the Project asset folder for your Font.
     */
    private void Start()
    {
        objectToActivate.SetActive(false);
        // Try to set the Font to the slotted font in Inspector
        textToDisplay.font = fontAsset;
        

        if (debug)
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
