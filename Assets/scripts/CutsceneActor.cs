using UnityEngine;
using System.Collections;

[System.Serializable]
public class CutsceneKeyframe
{
    public Transform targetTransform;
    public float duration = 2f;
    public string animTrigger;
}

public class CutsceneActor : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Configurações do Ator")]
    public float delayInicial = 0f;
    public Animator atorAnimator;

    [Header("Guião")]
    public CutsceneKeyframe[] rotina;

    // ---------------------------------------------
    //  CONTROLO DE ANIMACAO
    // ---------------------------------------------
    public void IniciarAcao()
    {
        StartCoroutine(ExecutarRotina());
    }

    private IEnumerator ExecutarRotina()
    {
        if (delayInicial > 0)
            yield return new WaitForSeconds(delayInicial);

        foreach (CutsceneKeyframe passo in rotina)
        {
            if (atorAnimator != null && !string.IsNullOrEmpty(passo.animTrigger))
            {
                atorAnimator.SetTrigger(passo.animTrigger);
            }

            if (passo.targetTransform != null)
            {
                Vector3 posInicial = transform.position;
                Quaternion rotInicial = transform.rotation;
                float passado = 0f;

                while (passado < passo.duration)
                {
                    passado += Time.deltaTime;
                    float progresso = passado / passo.duration;

                    transform.position = Vector3.Lerp(posInicial, passo.targetTransform.position, progresso);
                    transform.rotation = Quaternion.Slerp(rotInicial, passo.targetTransform.rotation, progresso);

                    yield return null;
                }

                transform.position = passo.targetTransform.position;
                transform.rotation = passo.targetTransform.rotation;
            }
            else
            {
                yield return new WaitForSeconds(passo.duration);
            }
        }
    }
}
