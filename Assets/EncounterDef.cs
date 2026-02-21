using UnityEngine;

[CreateAssetMenu(menuName = "Game/Encounter Def")]
public class EncounterDef : ScriptableObject
{
    [Header("Dialogue")]
    public NPCDialogue dialogue;

    [Header("Battle")]
    public EnemyStats enemy;

    [Tooltip("If true, a battle starts after dialogue ends")]
    public bool startsBattle = true;

    [Header("Stat Overrides (0 = use default)")]
    [Tooltip("Overrides enemy max HP for this encounter")]
    public int enemyHPOverride = 0;

    [Tooltip("Overrides enemy damage for this encounter")]
    public int enemyDamageOverride = 0;

    public enum CompanionOutcomeMode
    {
        MatchOutcome,     // Win->Happy, Lose->Sad
        MismatchOutcome,  // Win->Sad,   Lose->Happy
        ForceIdle,
        ForceHappy,
        ForceSad
    }

    public enum FixedOutcome
    {
        WinByALot,
        Win,
        Lose,
        LoseByALot
    }

    public FixedOutcome fixedOutcome = FixedOutcome.Win;


    public CompanionOutcomeMode companionOutcomeMode = CompanionOutcomeMode.MatchOutcome;
}
