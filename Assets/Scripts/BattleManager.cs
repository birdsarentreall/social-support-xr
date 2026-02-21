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

    bool blockNextHit;
    bool blockedLastTurn;

    int enemyTurnsRemaining;
    int damageBudgetRemaining;

    bool battleEnd;


    void OnEnable()
    {
        if (string.IsNullOrEmpty(sessionId))
            sessionId = System.Guid.NewGuid().ToString("N");
        StartBattle();
    }

    void StartBattle()
    {
        battleEnd = false;
        CancelInvoke();

        battleIndex++;
        battleStartTime = Time.time;
        blockNextHit = false;

        // Player HP is always fixed
        playerHP = playerMaxHP;
        // Director: pick target final HP + number of enemy turns based on fixed outcome
        int targetFinalHP = 5;           // Win default
        enemyTurnsRemaining = 4;         // Win default

        var enc = BattleContext.CurrentEncounter;
        if (enc != null)
        {
            switch (enc.fixedOutcome)
            {
                case EncounterDef.FixedOutcome.WinByALot:  targetFinalHP = 9; enemyTurnsRemaining = 3; break;
                case EncounterDef.FixedOutcome.Win:        targetFinalHP = 5; enemyTurnsRemaining = 4; break;
                case EncounterDef.FixedOutcome.Lose:       targetFinalHP = 0; enemyTurnsRemaining = 4; break;
                case EncounterDef.FixedOutcome.LoseByALot:  targetFinalHP = 0; enemyTurnsRemaining = 2; break;
            }
        }

        damageBudgetRemaining = Mathf.Max(0, playerHP - targetFinalHP);

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
        if (battleEnd) return;
        if (!playerTurn) return;
        blockedLastTurn = false;

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

    public void ConfirmBlock()
    {
        if (!playerTurn) return;

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
        int rawDmg = ChoosePlannedEnemyDamage(); // planned 0–2

        int finalDmg = rawDmg;

        if (blockNextHit)
        {
            finalDmg = 0;
            blockNextHit = false;

            damageBudgetRemaining += rawDmg;

            messageText.SetText("Blocked!");
        }
        else
        {
            messageText.SetText(rawDmg >= 2 ? "Enemy attacks (Heavy)!" : "Enemy attacks (Light)!");
        }

        playerHP -= finalDmg;
        if (playerHP < 0) playerHP = 0;

        if (companion) companion.UpdateDuringBattle(playerHP, playerMaxHP);
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
        if (battleEnd) return;
        battleEnd = true;

        CancelInvoke();
        playerTurn = false; 
        float durationSeconds = Time.time - battleStartTime;

        var enc = BattleContext.CurrentEncounter;
        string encounterName = enc ? enc.name : "NULL";
        string outcome = playerWon ? "WIN" : "LOSE";

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

    int ChoosePlannedEnemyDamage()
    {
        if (enemyTurnsRemaining <= 0)
        {
            int spend = Mathf.Clamp(damageBudgetRemaining, 0, 2);
            damageBudgetRemaining -= spend;
            return spend;
        }

        float requiredAvg = damageBudgetRemaining / (float)enemyTurnsRemaining;

        int planned = Mathf.Clamp(Mathf.RoundToInt(requiredAvg), 1, 2);

        enemyTurnsRemaining--;
        damageBudgetRemaining -= planned;
        if (damageBudgetRemaining < 0) damageBudgetRemaining = 0;

        return planned;
    }

    IEnumerator EndBattleAfterReaction(bool playerWon)
    {
        yield return new WaitForSeconds(companion.outcomeReactSeconds);

        EncounterSequence.Instance.OnBattleFinished(playerWon);
        BattleContext.CurrentEncounter = null;
        GameStateTransition.Instance.EndBattle(playerWon);
    }

}

