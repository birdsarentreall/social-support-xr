using UnityEngine;
using TMPro;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [Header("Stats")]
    public int playerMaxHP = 10;
    public int enemyMaxHP = 3;
    public int enemyDamage = 1;

    int playerHP;
    int enemyHP;

    [Header("UI")]
    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;
    public TMP_Text messageText;

    [Header("Spectator")]
    public CompanionEmotionController companion;

    bool playerTurn;
    int battleIndex = 0;
    float battleStartTime;
    string sessionId;



    void OnEnable()
    {
        if (string.IsNullOrEmpty(sessionId))
            sessionId = System.Guid.NewGuid().ToString("N");
        StartBattle();
    }

    void StartBattle()
    {
        battleIndex++;
        var enc = BattleContext.CurrentEncounter;

        // Player HP is always fixed
        playerHP = playerMaxHP;

        if (enc == null || enc.enemy == null)
        {
            Debug.LogError("BattleContext.CurrentEncounter is missing.");
            enemyMaxHP = 3;
            enemyDamage = 1;
        }
        else
        {
            // Enemy stats (can vary per encounter)
            enemyMaxHP = (enc.enemyHPOverride > 0)
                ? enc.enemyHPOverride
                : enc.enemy.maxHP;

            enemyDamage = (enc.enemyDamageOverride > 0)
                ? enc.enemyDamageOverride
                : enc.enemy.damage;
        }

        enemyHP = enemyMaxHP;
        playerTurn = true;

        UpdateUI();
        messageText.SetText("Your turn");
        if (companion) companion.UpdateDuringBattle(playerHP, playerMaxHP);
    }

    public void ConfirmAttack()
    {
        //Debug.Log("attac");
        PlayerAttack();
    }


    public void PlayerAttack()
    {
        if (!playerTurn) return;

        enemyHP -= 1;
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

    void EnemyAttack()
    {
        playerHP -= 1;
        messageText.SetText("Enemy attacks!");
        companion.UpdateDuringBattle(playerHP, playerMaxHP);

        UpdateUI();

        if (playerHP <= 0)
        {
            messageText.SetText("You lose...");
            EndBattle(false);
            return;
        }

        playerTurn = true;
        messageText.SetText("Your turn");
    }

    void UpdateUI()
    {
        playerHPText.SetText($"HP: {playerHP}");
        enemyHPText.SetText($"HP: {enemyHP}");
    }

    void EndBattle(bool playerWon)
    {
        float durationSeconds = Time.time - battleStartTime;

        var enc = BattleContext.CurrentEncounter;
        string encounterName = enc ? enc.name : "NULL";
        string outcome = playerWon ? "WIN" : "LOSE";

        // Your emotion condition (nested enum version)
        string emotionMode = (enc != null) ? enc.companionOutcomeMode.ToString() : "NULL";

        BattleMetricsLogger.AppendBattleRow(
            sessionId,
            battleIndex,
            encounterName,
            emotionMode,
            outcome,
            durationSeconds
        );

        var mode = EncounterDef.CompanionOutcomeMode.MatchOutcome;

        if (enc != null) mode = enc.companionOutcomeMode;

        int shownEmotion = 0;

        switch (mode)
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
            case EncounterDef.CompanionOutcomeMode.ForceHappy:
                shownEmotion = 1;
                break;
            case EncounterDef.CompanionOutcomeMode.ForceSad:
                shownEmotion = 2;
                break;
        }

        companion.ReactToOutcomeForced(shownEmotion);
        StartCoroutine(EndBattleAfterReaction(playerWon));
    }


    IEnumerator EndBattleAfterReaction(bool playerWon)
    {
        yield return new WaitForSeconds(companion.outcomeReactSeconds);

        EncounterSequence.Instance.OnBattleFinished(playerWon);
        BattleContext.CurrentEncounter = null;
        GameStateTransition.Instance.EndBattle(playerWon);
    }

}

