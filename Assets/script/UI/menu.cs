using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject menuUI;
    private bool isShowing = false;
    void Start()
    { 
        menuUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isShowing)
            {
                menuUI.SetActive(true);
                isShowing = true;
            }
            else
            {
                menuUI.SetActive(false);
                isShowing= false;
            }
        }

    }
}
