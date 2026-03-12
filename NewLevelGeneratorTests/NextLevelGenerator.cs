using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace projecthorizonscs.NewLevelGeneratorTests;

public sealed class ChunkData(List<int> layer0, List<int> layer1)
{
    public readonly List<int> Layer0 = layer0;
    public readonly List<int> Layer1 = layer1;
}

public readonly record struct GeneratedChunk(Vector2I Position, ChunkData Data);

public partial class NextLevelGenerator : TileMapLayer
{
    private FastNoiseLite.NoiseTypeEnum _layer0NoiseType = FastNoiseLite.NoiseTypeEnum.ValueCubic;
    private int _layer0Seed;
    private float _layer0Frequency = .01f;
    private int _layer0FractalOctaves = 8;

    private float _insideDetails = 40f;
    private float _coastDetails = 90.0f;
    private float _coastStrength = .55f;
    private float _threshold = .45f;

    private int _chunkSizeX = 24;
    private int _chunkSizeY = 74;

    private int _levelSizeX = 2000;
    private int _levelSizeY = 2800;

    private Vector2I _currentPlayerChunk = Vector2I.Zero;
    private Vector2I _lastRenderedChunk = new(int.MinValue, int.MinValue);

    private const int SourceId = 0;
    private const int RenderDistance = 1;
    private const int MaxChunkThreads = 2;

    private readonly Dictionary<Vector2I, ChunkData> _chunksDataDictionary = new();
    private readonly HashSet<Vector2I> _renderedChunks = new();

    // chunks sendo gerados agora
    private readonly HashSet<Vector2I> _pendingChunks = new();
    private readonly object _pendingChunksLock = new();

    // fila thread-safe de chunks prontos
    private readonly ConcurrentQueue<GeneratedChunk> _generatedChunksQueue = new();

    // só pra limitar quantas tasks rodam ao mesmo tempo
    private int _runningChunkThreads = 0;
    private readonly object _threadCountLock = new();

    private readonly Dictionary<int, Vector2I> _blockIDs = new()
    {
        { 0, new Vector2I(0, 0) }, // void
        { 1, new Vector2I(1, 0) }, // half-void
        { 2, new Vector2I(0, 1) }, // grass
        { 3, new Vector2I(0, 2) }, // dirt
    };

    public override void _Ready()
    {
        SetNoiseSeed();
        UpdatePlayerChunk();
        RequestChunksAroundPlayer(forceRenderLoaded: true);
    }

    public override void _Process(double delta)
    {
        UpdatePlayerChunk();
        ConsumeGeneratedChunks();

        if (_currentPlayerChunk != _lastRenderedChunk)
        {
            RequestChunksAroundPlayer(forceRenderLoaded: false);
            _lastRenderedChunk = _currentPlayerChunk;
        }
    }

    private void UpdatePlayerChunk()
    {
        if (Autoload.Globals.I?.LocalPlayer == null)
            return;

        Vector2I playerMapPos = LocalToMap(ToLocal(Autoload.Globals.I.LocalPlayer.GlobalPosition));

        _currentPlayerChunk = new Vector2I(
            Mathf.FloorToInt((float)playerMapPos.X / _chunkSizeX),
            Mathf.FloorToInt((float)playerMapPos.Y / _chunkSizeY)
        );

        Autoload.Globals.I.CurrentPlayerChunk = _currentPlayerChunk;
    }

    private void RequestChunksAroundPlayer(bool forceRenderLoaded)
    {
        HashSet<Vector2I> desiredChunks = new();

        for (int y = -RenderDistance; y <= RenderDistance; y++)
        {
            for (int x = -RenderDistance; x <= RenderDistance; x++)
            {
                Vector2I chunkPos = _currentPlayerChunk + new Vector2I(x, y);
                desiredChunks.Add(chunkPos);

                if (!IsChunkInsideBounds(chunkPos))
                    continue;

                if (_chunksDataDictionary.ContainsKey(chunkPos))
                {
                    if (forceRenderLoaded || !_renderedChunks.Contains(chunkPos))
                        RenderChunk(chunkPos);

                    continue;
                }

                RequestChunkGeneration(chunkPos);
            }
        }

        _renderedChunks.Clear();
        foreach (Vector2I chunk in desiredChunks)
            _renderedChunks.Add(chunk);
    }

    private bool IsChunkInsideBounds(Vector2I chunkPos)
    {
        int chunksX = _levelSizeX / _chunkSizeX;
        int chunksY = _levelSizeY / _chunkSizeY;

        return chunkPos.X >= -chunksX && chunkPos.X < chunksX &&
               chunkPos.Y >= -chunksY && chunkPos.Y < chunksY;
    }

    private void RequestChunkGeneration(Vector2I chunkPos)
    {
        if (_chunksDataDictionary.ContainsKey(chunkPos))
            return;

        lock (_pendingChunksLock)
        {
            if (_pendingChunks.Contains(chunkPos))
                return;
        }

        lock (_threadCountLock)
        {
            if (_runningChunkThreads >= MaxChunkThreads)
                return;

            _runningChunkThreads++;
        }

        lock (_pendingChunksLock)
        {
            _pendingChunks.Add(chunkPos);
        }

        Task.Run(() =>
        {
            try
            {
                ChunkData data = GenerateChunkDataThreadSafe(chunkPos);
                _generatedChunksQueue.Enqueue(new GeneratedChunk(chunkPos, data));
            }
            finally
            {
                lock (_pendingChunksLock)
                {
                    _pendingChunks.Remove(chunkPos);
                }

                lock (_threadCountLock)
                {
                    _runningChunkThreads--;
                }
            }
        });
    }

    private void ConsumeGeneratedChunks()
    {
        while (_generatedChunksQueue.TryDequeue(out GeneratedChunk generated))
        {
            if (_chunksDataDictionary.ContainsKey(generated.Position))
                continue;

            _chunksDataDictionary[generated.Position] = generated.Data;
            RenderChunk(generated.Position);
        }
    }

    private void RenderChunk(Vector2I chunkPosition)
    {
        if (!_chunksDataDictionary.TryGetValue(chunkPosition, out ChunkData currentChunk))
            return;

        int originX = chunkPosition.X * _chunkSizeX;
        int originY = chunkPosition.Y * _chunkSizeY;

        int i = 0;

        for (int y = 0; y < _chunkSizeY; y++)
        {
            for (int x = 0; x < _chunkSizeX; x++)
            {
                int id = currentChunk.Layer0[i++];
                Vector2I cell = new(originX + x, originY + y);

                if (id == -1)
                {
                    EraseCell(cell);
                    continue;
                }

                if (_blockIDs.TryGetValue(id, out Vector2I atlasCoords))
                    SetCell(cell, SourceId, atlasCoords);
            }
        }
    }

    private ChunkData GenerateChunkDataThreadSafe(Vector2I gridCoordinates)
    {
        long start = Time.GetTicksMsec();

        // cria noise local da thread
        FastNoiseLite threadNoise = new()
        {
            NoiseType = _layer0NoiseType,
            Seed = _layer0Seed,
            FractalOctaves = _layer0FractalOctaves,
            Frequency = _layer0Frequency
        };

        List<int> newChunkLayer0Data = new(_chunkSizeX * _chunkSizeY);

        float levelRadius = Mathf.Min(_levelSizeX, _levelSizeY) * .42f;

        int originX = gridCoordinates.X * _chunkSizeX;
        int originY = gridCoordinates.Y * _chunkSizeY;

        for (int y = 0; y < _chunkSizeY; y++)
        {
            for (int x = 0; x < _chunkSizeX; x++)
            {
                Vector2I global = new(originX + x, originY + y);

                float dist = global.Length();
                float angle = Mathf.Atan2(global.Y, global.X);

                float ax = Mathf.Cos(angle) * _coastDetails;
                float ay = Mathf.Sin(angle) * _coastDetails;

                float coastN = threadNoise.GetNoise2D(ax + 123.4f, ay - 567.8f);
                float coast01 = (coastN + 1f) * .5f;
                float radius = levelRadius * (1f + (coast01 - .5f) * 2f * _coastStrength);

                float mask = 1f - (dist / radius);
                mask = Mathf.Clamp(mask, 0f, 1f);
                mask *= mask;

                float n = threadNoise.GetNoise2D(global.X * _insideDetails, global.Y * _insideDetails);
                float inside01 = (n + 1f) * .5f;

                float value = mask * (.65f + inside01 * .35f);

                if (value > _threshold)
                    newChunkLayer0Data.Add(1);
                else if (value > _threshold - .05f)
                    newChunkLayer0Data.Add(2);
                else
                    newChunkLayer0Data.Add(-1);
            }
        }

        long end = Time.GetTicksMsec();
        GD.Print($"chunk system: chunk {gridCoordinates} generated in background in {end - start} ms");

        return new ChunkData(newChunkLayer0Data, new List<int>());
    }

    private void SetNoiseSeed()
    {
        RandomNumberGenerator rng = new();
        _layer0Seed = rng.RandiRange(0, 99999999);
    }
}