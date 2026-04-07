using UnityEngine;
using System.Collections.Generic;

public enum AttackType { Light, Heavy }
public enum Outcome { BigWin, Win, Loss, BigLoss }

[CreateAssetMenu(menuName="Game/Enemy Def")]
public class EnemyStats : ScriptableObject
{
    public string enemyName;
    public Sprite enemyPortrait;

    [Header("Health")]
    public int maxHP = 100;

    [Header("Damage")]
    public int lightDamage = 12;
    public int heavyDamage = 28;

    [Header("Block")]
    public float heavyBlockMultiplier = 0.5f;

    [Header("Enemy Pattern")]
    public List<AttackType> attackPattern = new();
}