using UnityEngine;

[CreateAssetMenu(menuName="Game/Enemy Def")]
public class EnemyStats : ScriptableObject
{
    public string enemyName;
    public Sprite enemyPortrait;

    [Header("Stats")]
    public int maxHP = 3;
    public int damage = 1;
}
