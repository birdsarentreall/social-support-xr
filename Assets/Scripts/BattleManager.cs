using UnityEngine;
using TMPro;

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

    bool playerTurn;

    void OnEnable()
    {
        StartBattle();
    }

void StartBattle()
{
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
        EncounterSequence.Instance.OnBattleFinished(playerWon);
        BattleContext.CurrentEncounter = null;
        GameStateTransition.Instance.EndBattle(playerWon);
    }

}

