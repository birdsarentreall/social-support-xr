using UnityEngine;
using TMPro;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [Header("Stats")]
    public int playerMaxHP = 100;
    public int enemyMaxHP = 3;
    public int enemyDamage = 1;

    int playerHP;
    int enemyHP;

    EnemyStats enemy;
    int lightDamage;
    int heavyDamage;
    float heavyBlockMultiplier;
    int patternIndex;

    AttackType nextEnemyAttack;   
    [Header("UI")]
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;
    public TMP_Text messageText;

    [Header("Spectator")]
    public CompanionEmotionController companion;

    bool playerTurn;
    bool battleEnd;

    bool blockNextHit;
    bool blockedLastTurn;

    public GlanceTracker lookTracker;

    int battleIndex = 0;
    float battleStartTime;
    string sessionId;

    [Header("Intent Indicator")]
    public TMP_Text enemyIntentText;     

    void OnEnable()
    {
        if (string.IsNullOrEmpty(sessionId))
            sessionId = System.Guid.NewGuid().ToString("N");

        StartBattle();
    }

    void StartBattle()
    {
        if (lookTracker) lookTracker.ResetCount();
        battleEnd = false;
        CancelInvoke();

        battleIndex++;
        battleStartTime = Time.time;

        blockNextHit = false;
        blockedLastTurn = false;

        var enc = BattleContext.CurrentEncounter;

        playerHP = playerMaxHP;

        if (enc == null || enc.enemy == null)
        {
            Debug.LogError("BattleContext.CurrentEncounter is missing.");
            enemy = null;

            enemyMaxHP = 100;
            lightDamage = 12;
            heavyDamage = 28;
            heavyBlockMultiplier = 0.5f;
            patternIndex = 0;
        }
        else
        {
            enemy = enc.enemy;

            enemyMaxHP = (enc.enemyHPOverride > 0) ? enc.enemyHPOverride : enemy.maxHP;

            lightDamage = enemy.lightDamage;
            heavyDamage = enemy.heavyDamage;

            heavyBlockMultiplier = enemy.heavyBlockMultiplier;
            patternIndex = 0;
        }

        enemyHP = enemyMaxHP;
        playerTurn = true;


        NextEnemyAttack();
        UpdateUI();
        messageText.SetText("Your turn");

        if (companion)
            companion.UpdateDuringBattle(playerHP, playerMaxHP);
    }

    public void ConfirmAttack()
    {
        if (battleEnd || !playerTurn) return;

        blockedLastTurn = false;

        enemyHP -= 20;
        enemyHP = Mathf.Max(0, enemyHP);

        messageText.SetText("You attack!");
        UpdateUI();

        if (enemyHP <= 0)
        {
            messageText.SetText("Enemy defeated!");
            EndBattle(true);
            return;
        }

        playerTurn = false;
        Invoke(nameof(EnemyAttack), 1.0f);
    }

    public void ConfirmBlock()
    {
        if (battleEnd || !playerTurn) return;

        if (blockedLastTurn)
        {
            messageText.SetText("You can't block twice in a row!");
            return;
        }

        blockNextHit = true;
        blockedLastTurn = true;

        playerTurn = false;
        messageText.SetText("You block!");

        Invoke(nameof(EnemyAttack), 1.0f);
    }

    void EnemyAttack()
    {
        if (battleEnd) return;

        int finalDamage = 0;

        if (nextEnemyAttack == AttackType.Light)
        {
            finalDamage = lightDamage;

            if (blockNextHit)
            {
                finalDamage = 0;
                messageText.SetText("Blocked light attack!");
            }
            else
            {
                messageText.SetText("Enemy uses light attack!");
            }
        }
        else
        {
            finalDamage = heavyDamage;

            if (blockNextHit)
            {
                finalDamage = Mathf.RoundToInt(finalDamage * heavyBlockMultiplier);
                messageText.SetText("Blocked heavy attack!");
            }
            else
            {
                messageText.SetText("Enemy uses heavy attack!");
            }
        }

        blockNextHit = false;

        playerHP -= finalDamage;
        playerHP = Mathf.Max(0, playerHP);

        if (companion)
            companion.UpdateDuringBattle(playerHP, playerMaxHP);

        if (playerHP <= 0)
        {
            playerHP = 0;
            UpdateUI();
            messageText.SetText("You lose...");
            EndBattle(false);
            return;
        }

        NextEnemyAttack();
        UpdateUI();

        playerTurn = false;
        CancelInvoke(nameof(BeginPlayerTurn));
        Invoke(nameof(BeginPlayerTurn), 0.8f);
    }

    void BeginPlayerTurn()
        {
            if (battleEnd) return;
            playerTurn = true;
            messageText.SetText("Your turn");
        }

    void NextEnemyAttack()
    {
        if (enemy == null || enemy.attackPattern == null || enemy.attackPattern.Count == 0)
        {
            nextEnemyAttack = AttackType.Light;
            return;
        }

        nextEnemyAttack = enemy.attackPattern[patternIndex % enemy.attackPattern.Count];
        patternIndex++;
    }

    void UpdateUI()
    {
        playerHPText.SetText($"HP: {playerHP}");
        enemyHPText.SetText($"HP: {enemyHP}");
        enemyIntentText.SetText(nextEnemyAttack == AttackType.Light ? "Light" : "Heavy");
    }

    void EndBattle(bool playerWon)
    {
        if (battleEnd) return;
        battleEnd = true;

        CancelInvoke();
        playerTurn = false;

        float durationSeconds = Time.time - battleStartTime;
        int spectatorLooks = lookTracker ? lookTracker.lookCount : 0;

        var enc = BattleContext.CurrentEncounter;
        string encounterName = enc ? enc.name : "NULL";
        string outcome = playerWon ? "WIN" : "LOSE";
        string emotionMode = enc ? enc.companionOutcomeMode.ToString() : "NULL";

        BattleMetricsLogger.AppendBattleRow(
            sessionId,
            battleIndex,
            encounterName,
            emotionMode,
            outcome,
            durationSeconds,
            spectatorLooks
        );

        int shownEmotion = 0;

        if (enc != null)
        {
            switch (enc.companionOutcomeMode)
            {
                case EncounterDef.CompanionOutcomeMode.MatchOutcome:
                    shownEmotion = playerWon ? 1 : 2;
                    break;
                case EncounterDef.CompanionOutcomeMode.MismatchOutcome:
                    shownEmotion = playerWon ? 2 : 1;
                    break;
                case EncounterDef.CompanionOutcomeMode.ForceIdle:
                    shownEmotion = 0;
                    break;
            }
        }

        if (companion)
            companion.ReactToOutcomeForced(shownEmotion);

        StartCoroutine(EndBattleAfterReaction(playerWon));
    }

    IEnumerator EndBattleAfterReaction(bool playerWon)
    {
        float wait = companion ? companion.outcomeReactSeconds : 0f;
        yield return new WaitForSeconds(wait);

        EncounterSequence.Instance.OnBattleFinished(playerWon);
        BattleContext.CurrentEncounter = null;
        GameStateTransition.Instance.EndBattle(playerWon);
    }
}