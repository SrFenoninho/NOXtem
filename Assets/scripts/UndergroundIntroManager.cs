using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UndergroundIntroManager : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Player & Camera")]

    public FPMove playerMovement;
    public Transform playerCamera;

    [Header("Audio")]
    public AudioClip firstAudio;
    public AudioClip secondAudio;
    public AudioClip punchAudio;




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private AudioSource audioSource;

    [Header("Elevator Doors")]
    public Transform door1;
    public Transform door2;
    public Vector3 doorMoveDirection = Vector3.right;
    public float jammedDistance = 0.5f;
    public float openDuration = 1f;

    [Header("Punch Settings")]
    public float punchForce = 1500f;

    [Header("UI")]
    public TMP_Text messageText;

    [Header("Timings")]
    public float shakeDuration = 4f;
    public float shakeMagnitude = 0.05f;

    private bool waitingForPunch = false;
    private Vector3 cameraOriginalLocalPos;

    private Vector3 door1StartPos;
    private Vector3 door2StartPos;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (playerCamera != null)
            cameraOriginalLocalPos = playerCamera.localPosition;

        if (door1 != null) door1StartPos = door1.localPosition;
        if (door2 != null) door2StartPos = door2.localPosition;

        if (messageText != null) messageText.text = "";

        StartCutscene();
    }

    void Update()
    {
        if (waitingForPunch && Input.GetMouseButtonDown(0))
        {
            PerformPunch();
        }
    }




    // ---------------------------------------------
    //  PRIVATE METHODS
    // ---------------------------------------------
    void StartCutscene()
    {
        if (playerMovement != null) playerMovement.inputBlocked = true;

        GameStateManager.Instance?.PushState(GameState.Cutscene);

        StartCoroutine(IntroSequenceRoutine());
    }

    IEnumerator IntroSequenceRoutine()
    {
        if (firstAudio != null)
        {
            audioSource.clip = firstAudio;
            audioSource.loop = false;
            audioSource.Play();
        }

        while (audioSource.isPlaying)
        {
            if (playerCamera != null)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                playerCamera.localPosition = new Vector3(cameraOriginalLocalPos.x + x, cameraOriginalLocalPos.y + y, cameraOriginalLocalPos.z);
            }
            yield return null;
        }

        if (playerCamera != null)
            playerCamera.localPosition = cameraOriginalLocalPos;

        audioSource.Stop();
        float audioDuration = 0f;
        if (secondAudio != null)
        {
            audioSource.PlayOneShot(secondAudio);
            audioDuration = secondAudio.length;
        }

        yield return StartCoroutine(OpenDoorsJammedRoutine());

        if (audioDuration > openDuration)
        {
            yield return new WaitForSeconds(audioDuration - openDuration);
        }

        if (messageText != null)
        {
            messageText.text = "Left Click to force the door open";
        }

        waitingForPunch = true;
    }

    IEnumerator OpenDoorsJammedRoutine()
    {
        float elapsed = 0f;

        Vector3 moveDirNorm = doorMoveDirection.normalized;
        Vector3 door1TargetPos = door1StartPos + (moveDirNorm * jammedDistance);
        Vector3 door2TargetPos = door2StartPos + (moveDirNorm * jammedDistance); 

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float smoothStep = Mathf.SmoothStep(0f, 1f, t);

            if (door1 != null)
                door1.localPosition = Vector3.Lerp(door1StartPos, door1TargetPos, smoothStep);

            if (door2 != null)
                door2.localPosition = Vector3.Lerp(door2StartPos, door2TargetPos, smoothStep);

            yield return null;
        }
    }

    void PerformPunch()
    {
        waitingForPunch = false;

        if (punchAudio != null)
            audioSource.PlayOneShot(punchAudio);

        if (messageText != null)
            messageText.text = "";

        BlowDoorAway(door1);
        if (door1 != null)
            Destroy(door1.gameObject, 15f);

        if (playerMovement != null) playerMovement.inputBlocked = false;
        GameStateManager.Instance?.PopState();
    }

    void BlowDoorAway(Transform doorTransform)
    {
        if (doorTransform == null) return;

        Rigidbody rb = doorTransform.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = doorTransform.gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;

        Vector3 forceDirection = (playerCamera != null) ? playerCamera.forward : Vector3.forward;
        forceDirection.y += 0.5f;

        rb.AddForce(forceDirection.normalized * punchForce, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-50f, 50f), Random.Range(-50f, 50f), Random.Range(-50f, 50f)), ForceMode.Impulse);
    }
}
