using UnityEngine;

public class TransformationManager : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Players")]
    public GameObject fpPlayer;         // Be
    public GameObject tpPlayer;         // w - nunca SetActive(false)

    [Header("Cameras")]
    public Camera fpCamera;
    public Camera tpCamera;

    [Header("Modelo TP - esconder em FP")]
    public Renderer[] tpRenderers;      // MeshRenderers do w

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
    private float lastTransformTime = -999f;
    private AudioSource audioSource;

    [Header("Input")]
    public KeyCode transformKey = KeyCode.T;

    [HideInInspector] public bool transformationBlocked = false;


    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool isTPForm = true;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        // Apanhar renderers automaticamente se vazio
        if ((tpRenderers == null || tpRenderers.Length == 0) && tpPlayer != null)
            tpRenderers = tpPlayer.GetComponentsInChildren<Renderer>();

        // Desativar PlayerHealth do Be se existir
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
        // Em FP o w segue o Be (inimigos continuam a detetar o w)
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
            // So transforma dentro de zona de combate
            if (!CombatZone.InCombatZone) return;
            if (Time.time - lastTransformTime < transformCooldown) return;
            lastTransformTime = Time.time;
            if (transformSound != null) audioSource.PlayOneShot(transformSound);
            if (isTPForm) SwitchToFP();
            else SwitchToTP();
        }
    }

    // ---------------------------------------------
    //  TRANSFORMACAO
    // ---------------------------------------------
    void SwitchToFP()
    {
        // Teleportar Be para posicao do w
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
        // Teleportar w para posicao do Be
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

    // ---------------------------------------------
    //  ATIVAR ESTADOS
    // ---------------------------------------------
    void ActivateFP()
    {
        isTPForm = false;

        // Ativar Be
        if (fpPlayer != null) fpPlayer.SetActive(true);
        if (fpCamera != null) fpCamera.enabled = true;
        if (fpMove != null) fpMove.enabled = true;
        if (playerInteraction != null) playerInteraction.enabled = true;

        // Desativar scripts TP mas NAO o w (inimigos precisam dele)
        if (tpCamera != null) tpCamera.enabled = false;
        if (tpMove != null) tpMove.enabled = false;
        if (orbitCam != null) orbitCam.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;
        if (tpController != null) tpController.enabled = false;

        // Esconder modelo do w
        SetTPVisible(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ActivateTP()
    {
        isTPForm = true;

        // Desativar Be
        if (fpPlayer != null) fpPlayer.SetActive(false);
        if (fpCamera != null) fpCamera.enabled = false;
        if (fpMove != null) fpMove.enabled = false;
        if (playerInteraction != null) playerInteraction.enabled = false;

        // Ativar scripts TP
        if (tpCamera != null) tpCamera.enabled = true;
        if (tpMove != null) tpMove.enabled = true;
        if (orbitCam != null) orbitCam.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
        if (tpController != null) tpController.enabled = true;

        // Mostrar modelo do w
        SetTPVisible(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------------------------------------------
    //  AUXILIAR
    // ---------------------------------------------
    void SetTPVisible(bool visible)
    {
        if (tpRenderers == null) return;
        foreach (Renderer r in tpRenderers)
            if (r != null) r.enabled = visible;
    }

    // ---------------------------------------------
    //  FORCAR TRANSFORMACAO
    // ---------------------------------------------
    public void ForceTransformToTP()
    {
        if (isTPForm) return;
        SwitchToTP();
    }

    public bool IsTPForm() => isTPForm;
}