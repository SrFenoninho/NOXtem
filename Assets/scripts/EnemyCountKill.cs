using UnityEngine;
using System.Collections;

public class EnemyCountKill : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Objeto a Subir")]
    public Transform targetObject;
    public float riseHeight = 10f;
    public float riseSpeed = 1f;

    [Header("Cutscene do Portao")]
    public Camera cutsceneView;
    public float cutsceneDuration = 4f;
    public AudioClip gateMoveSound;

    [Header("Dialogo")]
    public AudioClip dialogueClip;
    public float slowMultiplier = 0.5f;

    [Header("Teleporte")]
    public Collider teleporterToActivate;

    [Header("Referencias")]
    public TransformationManager transformationManager;
    public MusicManager musicManager;
    public TPMove tpMove;
    public OrbitCam orbitCam;
    public Camera tpCamera;

    // ---------------------------------------------
    //  ESTADO PRIVADO
    // ---------------------------------------------
    private bool enemiesExisted = false;
    private bool triggered = false;
    private AudioSource audioSource;

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (cutsceneView != null)
            cutsceneView.enabled = false;

        if (teleporterToActivate != null)
            teleporterToActivate.enabled = false;
    }

    void Update()
    {
        if (triggered) return;

        int count = EnemyCombatManager.EnemyCount;

        if (!enemiesExisted && count > 0)
            enemiesExisted = true;

        if (enemiesExisted && count <= 1)
        {
            triggered = true;
            StartCoroutine(RewardSequence());
        }
    }

    // ---------------------------------------------
    //  SEQUENCIA COMPLETA
    // ---------------------------------------------
    IEnumerator RewardSequence()
    {
        // Bloquear inventario durante toda a sequencia
        GameStateManager.Instance?.PushState(GameState.Cutscene);

        if (transformationManager != null && !transformationManager.IsTPForm())
            transformationManager.ForceTransformToTP();

        if (transformationManager != null)
            transformationManager.transformationBlocked = true;

        if (tpMove != null)
            tpMove.inputBlocked = true;

        if (orbitCam != null)
            orbitCam.enabled = false;

        if (tpCamera != null && cutsceneView != null)
        {
            tpCamera.transform.position = cutsceneView.transform.position;
            tpCamera.transform.rotation = cutsceneView.transform.rotation;
        }

        if (musicManager != null)
            musicManager.StopMusic();

        if (targetObject != null)
        {
            if (gateMoveSound != null)
                audioSource.PlayOneShot(gateMoveSound);

            Vector3 startPos = targetObject.position;
            Vector3 endPos = startPos + Vector3.up * riseHeight;
            float elapsed = 0f;

            while (elapsed < cutsceneDuration)
            {
                elapsed += Time.deltaTime;
                targetObject.position = Vector3.MoveTowards(targetObject.position, endPos, riseSpeed * Time.deltaTime);
                yield return null;
            }
            targetObject.position = endPos;
        }
        else
        {
            yield return new WaitForSeconds(cutsceneDuration);
        }

        if (tpCamera != null && orbitCam != null)
        {
            orbitCam.enabled = true;

            float returnDuration = 1.5f;
            float t = 0f;
            Vector3 fromPos = tpCamera.transform.position;
            Quaternion fromRot = tpCamera.transform.rotation;

            orbitCam.enabled = false;

            while (t < returnDuration)
            {
                t += Time.deltaTime;
                float progress = t / returnDuration;
                progress = progress * progress * (3f - 2f * progress);

                Vector3 orbitTarget = orbitCam.target.position + orbitCam.targetOffset;
                Quaternion orbitRot = Quaternion.Euler(20f, orbitCam.target.eulerAngles.y, 0f);
                Vector3 orbitPos = orbitTarget - orbitRot * Vector3.forward * orbitCam.distance;

                tpCamera.transform.position = Vector3.Lerp(fromPos, orbitPos, progress);
                tpCamera.transform.rotation = Quaternion.Slerp(fromRot, Quaternion.LookRotation(orbitTarget - tpCamera.transform.position), progress);

                yield return null;
            }

            orbitCam.enabled = true;
        }

        if (tpMove != null)
            tpMove.inputBlocked = false;

        float origSpeed = 0f;
        float origSprint = 0f;

        if (tpMove != null)
        {
            origSpeed = tpMove.speed;
            origSprint = tpMove.sprintSpeed;
            tpMove.speed = origSpeed * slowMultiplier;
            tpMove.sprintSpeed = origSprint * slowMultiplier;
        }

        if (dialogueClip != null)
        {
            audioSource.PlayOneShot(dialogueClip);
            yield return new WaitForSeconds(dialogueClip.length);
        }

        if (tpMove != null)
        {
            tpMove.speed = origSpeed;
            tpMove.sprintSpeed = origSprint;
        }

        if (transformationManager != null)
            transformationManager.transformationBlocked = false;

        // Sequencia terminou - inventario pode voltar a abrir
        GameStateManager.Instance?.PopState();

        if (teleporterToActivate != null)
            teleporterToActivate.enabled = true;
    }
}
