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

    void Reset()
    {
        animator = GetComponent<Animator>();
    }

    // Call this anytime you want to set a “normal” emotion (no timer)
    public void SetIdle() => SetEmotion(0);
    public void SetHappy() => SetEmotion(1);
    public void SetSad() => SetEmotion(2);

    void SetEmotion(int e)
    {
        if (animator == null) return;
        animator.SetInteger(emotionParam, e);
    }

    // Call during battle whenever HP changes
    public void UpdateDuringBattle(int playerHP, int playerMaxHP)
    {
        if (overrideActive) return; // don’t fight post-battle override

        float frac = (playerMaxHP <= 0) ? 0f : (float)playerHP / playerMaxHP;

        if (frac <= sadThreshold) SetSad();
        else SetIdle();
    }

    // Call at battle end
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
}
