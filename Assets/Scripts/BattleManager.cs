using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Stats")]
    public int playerMaxHP = 5;
    public int enemyMaxHP = 3;

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
        playerHP = playerMaxHP;
        enemyHP = enemyMaxHP;
        playerTurn = true;

        UpdateUI();
        messageText.SetText("Your turn");
    }

    public void ConfirmAttack()
    {
        Debug.Log("attac");
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
            EndBattle();
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
            messageText.SetText("You were defeated...");
            EndBattle();
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

    void EndBattle()
    {
        // Placeholder: later return to exploration
        Debug.Log("Battle ended");
    }
}

