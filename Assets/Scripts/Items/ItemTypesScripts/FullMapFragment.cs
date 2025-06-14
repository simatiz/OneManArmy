using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Roguelike/ItemEffect/FullMap")]
public class FullMapEffect : ItemEffect
{
    public override void ApplyEffect(CharacterStatsBase stats)
    {
        Debug.Log("You possess the Full Map. Prepare to change the world!");
        CharacterStatsBase.maxHealth += 50f;
        CharacterStatsBase.CurrentHealth = CharacterStatsBase.maxHealth;
        CharacterStatsBase.level += 1;
        SceneManager.LoadScene($"Level{CharacterStatsBase.level}");
    }
}