using UnityEngine;

public class TransformationManager : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Players")]

    public GameObject fpPlayer;
    public GameObject tpPlayer;

    [Header("Cameras")]
    public Camera fpCamera;
    public Camera tpCamera;

    [Header("Modelo TP - esconder em FP")]
    public Renderer[] tpRenderers;

    [Header("Scripts FP")]
    public FPMove fpMove;
    public PlayerInteraction playerInteraction;

    [Header("Scripts TP")]
    public TPMove tpMove;
    public OrbitCam orbitCam;
    public PlayerCombat playerCombat;
    public CharacterController tpController;

    [Header("Transformacao")]
    public AudioClip transformSound;
    public float transformCooldown = 2f;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private float lastTransformTime = -999f;
    private AudioSource audioSource;

    [Header("Input")]
    public KeyCode transformKey = KeyCode.T;

    [HideInInspector] public bool transformationBlocked = false;

    private bool isTPForm = true;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if ((tpRenderers == null || tpRenderers.Length == 0) && tpPlayer != null)
            tpRenderers = tpPlayer.GetComponentsInChildren<Renderer>();

        if (fpPlayer != null)
        {
            PlayerHealth h = fpPlayer.GetComponent<PlayerHealth>();
            if (h != null) h.enabled = false;
        }

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        ActivateTP();
    }

    void Update()
    {
        if (!isTPForm && fpPlayer != null && tpPlayer != null)
        {
            if (tpController != null) tpController.enabled = false;
            tpPlayer.transform.position = fpPlayer.transform.position;
            if (tpController != null) tpController.enabled = true;
        }

        if (GameStateManager.Instance != null &&
            !GameStateManager.Instance.CanOpenRadialMenu()) return;

        if (Input.GetKeyDown(transformKey))
        {
            if (transformationBlocked) return;
            if (!CombatZone.InCombatZone) return;
            if (Time.time - lastTransformTime < transformCooldown) return;
            lastTransformTime = Time.time;
            if (transformSound != null) audioSource.PlayOneShot(transformSound);
            if (isTPForm) SwitchToFP();
            else SwitchToTP();
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void SwitchToFP()
    {
        CharacterController fpCC = fpPlayer != null ? fpPlayer.GetComponent<CharacterController>() : null;
        if (fpCC != null) fpCC.enabled = false;
        if (fpPlayer != null && tpPlayer != null)
        {
            fpPlayer.transform.position = tpPlayer.transform.position;
            fpPlayer.transform.rotation = Quaternion.Euler(0f, tpPlayer.transform.eulerAngles.y, 0f);
        }
        if (fpCC != null) fpCC.enabled = true;

        ActivateFP();
    }

    void SwitchToTP()
    {
        if (tpController != null) tpController.enabled = false;
        if (fpPlayer != null && tpPlayer != null)
        {
            tpPlayer.transform.position = fpPlayer.transform.position;
            tpPlayer.transform.rotation = Quaternion.Euler(0f, fpPlayer.transform.eulerAngles.y, 0f);
        }
        if (tpController != null) tpController.enabled = true;

        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        ActivateTP();
    }

    void ActivateFP()
    {
        isTPForm = false;

        if (fpPlayer != null) fpPlayer.SetActive(true);
        if (fpCamera != null) fpCamera.enabled = true;
        if (fpMove != null) fpMove.enabled = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        if (tpCamera != null) tpCamera.enabled = false;
        if (tpMove != null) tpMove.enabled = false;
        if (orbitCam != null) orbitCam.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (tpController != null) tpController.enabled = false;

        SetTPVisible(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ActivateTP()
    {
        isTPForm = true;

        if (fpPlayer != null) fpPlayer.SetActive(false);
        if (fpCamera != null) fpCamera.enabled = false;
        if (fpMove != null) fpMove.enabled = false;
        if (playerInteraction != null) playerInteraction.enabled = false;

        if (tpCamera != null) tpCamera.enabled = true;
        if (tpMove != null) tpMove.enabled = true;
        if (orbitCam != null) orbitCam.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
        if (tpController != null) tpController.enabled = true;

        SetTPVisible(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetTPVisible(bool visible)
    {
        if (tpRenderers == null) return;
        foreach (Renderer r in tpRenderers)
            if (r != null) r.enabled = visible;
    }





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void ForceTransformToTP()
    {
        if (isTPForm) return;
        SwitchToTP();
    }

    public bool IsTPForm() => isTPForm;
}
