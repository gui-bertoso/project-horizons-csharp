using Godot;
using projecthorizonscs.Enemys;
using System;
using System.Collections.Generic;

public partial class EnemysManager : Node
{
    public static EnemysManager I { get; private set; }

    public Node2D EnemysContainer { get; set; }

    public int EnemysAlive = 0;

    private readonly RandomNumberGenerator rng = new();
    private int _nextEnemyId = 1;

    private float _groupUpdateTimer = 0f;
    private const float GroupUpdateInterval = 0.2f;

    public Godot.Collections.Dictionary<int, EnemyGroup> LodGroups = new();
    private Godot.Collections.Array<EnemyTemplate> EnemysArray = new();

    public override void _Ready()
    {
        I = this;
        rng.Randomize();
    }

    public override void _Process(double delta)
    {
        _groupUpdateTimer -= (float)delta;

        if (_groupUpdateTimer > 0)
            return;

        _groupUpdateTimer = GroupUpdateInterval;

        UpdateEnemysCount();
        UpdateEnemyGroups();
    }

    public void SpawnEnemy(string enemyScenePath, Vector2 spawnPosition = default)
    {
        if (EnemysContainer == null || !GodotObject.IsInstanceValid(EnemysContainer))
        {
            GD.PushError("EnemysContainer is null.");
            return;
        }

        var enemyScene = GD.Load<PackedScene>(enemyScenePath);
        if (enemyScene == null)
        {
            GD.PushError($"Could not load enemy scene: {enemyScenePath}");
            return;
        }

        var newEnemy = enemyScene.Instantiate<EnemyTemplate>();
        EnemysContainer.AddChild(newEnemy);
        newEnemy.GlobalPosition = spawnPosition;

        newEnemy.ID = _nextEnemyId;
        _nextEnemyId++;

        EnemysArray.Add(newEnemy);
    }

    public void ClearEnemies()
    {
        foreach (var enemy in EnemysArray)
        {
            if (GodotObject.IsInstanceValid(enemy))
                enemy.QueueFree();
        }

        EnemysArray.Clear();
    }

    public void ClearInvalidEnemies()
    {
        for (int i = EnemysArray.Count - 1; i >= 0; i--)
        {
            if (!GodotObject.IsInstanceValid(EnemysArray[i]))
                EnemysArray.RemoveAt(i);
        }
    }

    public void ClearAllGroupEnemies()
    {
        foreach (var g in LodGroups.Values)
            g.ClearEnemys();
    }

    public void UpdateEnemyGroups()
    {
        ClearInvalidEnemies();
        ClearAllGroupEnemies();

        foreach (var enemy in EnemysArray)
        {
            var lod = GetEnemyLod(enemy.DistanceToPlayer);

            if (lod == 0)
                continue;

            var group = GetOrCreateGroup(lod);
            group.AddEnemy(enemy);
        }

        RemoveEmptyGroups();
    }

    public void RemoveEmptyGroups()
    {
        var toRemove = new List<int>();

        foreach (var kv in LodGroups)
        {
            if (kv.Value.enemysArray.Count == 0)
            {
                kv.Value.QueueFree();
                toRemove.Add(kv.Key);
            }
        }

        foreach (var key in toRemove)
            LodGroups.Remove(key);
    }

    public EnemyGroup GetOrCreateGroup(int lod)
    {
        if (lod == 0)
            return null;

        if (LodGroups.ContainsKey(lod))
            return LodGroups[lod];

        var group = new EnemyGroup();
        group.SetProcessLod(lod);

        AddChild(group);

        LodGroups[lod] = group;

        return group;
    }

    public int GetEnemyLod(float value)
    {
        if (value > 700f) return 3;
        if (value > 300f) return 2;
        if (value > 100f) return 1;
        return 0;
    }

    public void UpdateEnemysCount()
    {
        if (EnemysContainer == null || !GodotObject.IsInstanceValid(EnemysContainer))
        {
            EnemysAlive = 0;
            return;
        }

        EnemysAlive = EnemysContainer.GetChildCount();
    }
}