using System.Collections;
using UnityEngine;

public class CompanionEmotionController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;
    public string emotionParam = "Emotion";

    [Header("Thresholds")]
    [Tooltip("If player HP is <= this fraction of max, show Sad during battle.")]
    [Range(0f, 1f)] public float sadThreshold = 0.3f;

    [Header("Post-battle reaction")]
    public float outcomeReactSeconds = 2.0f;

    Coroutine overrideRoutine;
    bool overrideActive;
    void Start()
    {
        Debug.Log($"[CompanionEmotionController] START on {name}");
    }

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    public void SetIdle() => SetEmotion(0);
    public void SetHappy() => SetEmotion(1);
    public void SetSad() => SetEmotion(2);

    void SetEmotion(int e)
    {
        if (animator == null) return;
        animator.SetInteger(emotionParam, e);
    }

    public void UpdateDuringBattle(int playerHP, int playerMaxHP)
    {
        if (overrideActive) return;

        float frac = (playerMaxHP <= 0) ? 0f : (float)playerHP / playerMaxHP;

        if (frac <= sadThreshold) SetSad();
        else SetIdle();
    }

    public void ReactToOutcome(bool playerWon)
    {
        if (overrideRoutine != null) StopCoroutine(overrideRoutine);
        overrideRoutine = StartCoroutine(OutcomeRoutine(playerWon));
    }

    IEnumerator OutcomeRoutine(bool playerWon)
    {
        overrideActive = true;

        SetEmotion(playerWon ? 1 : 2);  // Happy on win, Sad on loss
        yield return new WaitForSeconds(outcomeReactSeconds);

        overrideActive = false;
        SetIdle();
    }

    public void ReactToOutcomeForced(int emotion) // 0 idle, 1 happy, 2 sad
    {
        if (overrideRoutine != null) StopCoroutine(overrideRoutine);
        overrideRoutine = StartCoroutine(OverrideRoutine(emotion));
    }

    IEnumerator OverrideRoutine(int emotion)
    {
        overrideActive = true;
        SetEmotion(emotion);
        yield return new WaitForSeconds(outcomeReactSeconds);
        overrideActive = false;
        SetIdle();
    }
}
