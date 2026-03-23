using Godot;
using projecthorizonscs.Enemys;
using System;
using System.Collections.Generic;

public partial class EnemysManager : Node
{
    public Godot.Collections.Dictionary<int, Godot.Collections.Array<EnemySpawnData>> enemyPathsToBiome = new()
    {
        {
            0,
            new()
            {
                new(25, "uid://cbi26ck84a7vt"),
                new(25, "uid://834fniewtsf1"),
                new(25, "uid://dsme77lawliia"),
                new(25, "uid://iqddsepl7qw2"),
            }
        }
    };
    
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
        Node spawnContainer = GetTree().CurrentScene;

        var enemyScene = GD.Load<PackedScene>(enemyScenePath);
        if (enemyScene == null)
        {
            GD.Print($"Could not load enemy scene: {enemyScenePath}");
            return;
        }

        if (EnemysContainer != null)
            spawnContainer = EnemysContainer;

        var newEnemy = enemyScene.Instantiate<EnemyTemplate>();

        newEnemy.ID = _nextEnemyId;
        _nextEnemyId++;

        spawnContainer.CallDeferred(Node.MethodName.AddChild, newEnemy);
        newEnemy.CallDeferred(Node2D.MethodName.SetGlobalPosition, spawnPosition);

        EnemysArray.Add(newEnemy);
        GD.Print($"EnemySpawner: spawned {spawnPosition}");
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

    public string GetRandomEnemyByChance(int BiomeId)
    {
        if (!enemyPathsToBiome.ContainsKey(BiomeId)) return null;

        var enemysList = enemyPathsToBiome[BiomeId];
        if (enemysList == null || enemysList.Count == 0) return null;

        int totalChance = 0;
        foreach (var enemy in enemysList)
            totalChance += enemy.Chance;
        
        if (totalChance <= 0)
            return null;
        
        int randomValue = GD.RandRange(1, totalChance);

        int currentChance = 0;

        foreach (var enemy in enemysList)
        {
            currentChance += enemy.Chance;

            if (randomValue <= currentChance)
                return enemy.Path;
        }

        return enemysList[0].Path;
    }
}