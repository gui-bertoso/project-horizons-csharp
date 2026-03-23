using Godot;
using System;

public partial class EnemySpawnData
{
    public int Chance;
    public string Path;

    public EnemySpawnData(int chance, string path)
    {
        Chance = chance;
        Path = path;
    }
}