using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodePanel : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Door Settings")]
    public SimpleLockedDoor doorToUnlock;
    public string correctCode = "1234";

    [Header("Visuals & UI")]
    public TMP_Text displayText;
    public Renderer panelRenderer;
    public Texture2D unlockedTexture;

    [Header("Interaction")]
    public float interactDistance = 3f;

    [System.Serializable]
    public struct CodeButton
    {
        public string digitValue;
        public Collider buttonCollider;
    }

    [Header("Buttons Setup")]
    public CodeButton[] buttons;

    [Header("Audio")]
    public AudioClip beepSound;
    public AudioClip errorSound;
    public AudioClip successSound;
    private AudioSource audioSource;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private string currentInput = "";
    private bool isUnlocked = false;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        
        UpdateDisplay();
    }

    void Update()
    {
        if (isUnlocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray;
            if (Cursor.lockState == CursorLockMode.None)
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            else
                ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                foreach (CodeButton btn in buttons)
                {
                    if (hit.collider == btn.buttonCollider)
                    {
                        OnButtonPressed(btn.digitValue);
                        break;
                    }
                }
            }
        }
    }

    // ---------------------------------------------
    //  LÓGICA DO CÓDIGO
    // ---------------------------------------------
    void OnButtonPressed(string digit)
    {
        if (beepSound != null) audioSource.PlayOneShot(beepSound);

        if (currentInput.Length < 4)
        {
            currentInput += digit;
            UpdateDisplay();
        }

        if (currentInput.Length == 4)
        {
            if (currentInput == correctCode)
            {
                UnlockPanel();
            }
            else
            {
                if (errorSound != null) audioSource.PlayOneShot(errorSound);
                currentInput = "";
                Invoke("UpdateDisplay", 0.5f);
            }
        }
    }

    void UpdateDisplay()
    {
        if (displayText == null) return;
        
        if (currentInput == "")
        {
            displayText.text = "0000";
        }
        else
        {
            displayText.text = currentInput.PadRight(4, '0');
        }
    }

    void UnlockPanel()
    {
        isUnlocked = true;

        if (successSound != null) audioSource.PlayOneShot(successSound);

        if (displayText != null)
            displayText.text = "OPEN";

        if (doorToUnlock != null)
            doorToUnlock.Unlock();

        if (panelRenderer != null && unlockedTexture != null)
            panelRenderer.material.mainTexture = unlockedTexture;
    }
}
