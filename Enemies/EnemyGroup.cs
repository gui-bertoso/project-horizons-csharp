using Godot;
using projecthorizonscs.Enemies;
using System.Collections.Generic;

public partial class EnemyGroup : Node
{
    public Dictionary<int, float> ProcessLodDictionary = new()
    {
        { 0, 0f },
        { 1, .2f },
        { 2, .4f },
        { 3, 1f }
    };

    public int CurrentProcessLod = 0;
    public Godot.Collections.Array<EnemyTemplate> enemysArray = new();
    public float UpdateCooldown = 0.2f;

    public void SetEnemies(Godot.Collections.Array<EnemyTemplate> enemysList)
    {
        enemysArray = enemysList;
        ApplyCooldownToAllEnemies();
    }

    public void SetProcessLod(int newLod)
    {
        if (!ProcessLodDictionary.ContainsKey(newLod))
            return;

        CurrentProcessLod = newLod;
        UpdateCooldown = ProcessLodDictionary[CurrentProcessLod];

        ApplyCooldownToAllEnemies();
    }

    public void ClearEnemies()
    {
        enemysArray.Clear();
    }

    public void AddEnemy(EnemyTemplate newEnemy)
    {
        if (enemysArray.Contains(newEnemy))
            return;

        enemysArray.Add(newEnemy);
        newEnemy.UpdateCooldown = UpdateCooldown;
    }

    public void RemoveEnemy(EnemyTemplate newEnemy)
    {
        enemysArray.Remove(newEnemy);
    }

    public void ClearInvalidEnemies()
    {
        for (int i = enemysArray.Count - 1; i >= 0; i--)
        {
            if (!GodotObject.IsInstanceValid(enemysArray[i]))
                enemysArray.RemoveAt(i);
        }
    }

    private void ApplyCooldownToAllEnemies()
    {
        foreach (var enemy in enemysArray)
        {
            if (GodotObject.IsInstanceValid(enemy))
                enemy.UpdateCooldown = UpdateCooldown;
        }
    }
}
