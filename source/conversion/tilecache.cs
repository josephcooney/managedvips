Here is the C# code equivalent to the given C code:

```csharp
using System;
using System.Collections.Generic;

public class VipsTileState
{
    public const int DATA = 0;
    public const int CALC = 1;
    public const int PEND = 2;
}

public class VipsTile
{
    public VipsBlockCache Cache { get; set; }
    public VipsTileState State { get; set; }
    public VipsRegion Region { get; set; }
    public int RefCount { get; set; }
    public VipsRect Pos { get; set; }

    public VipsTile(VipsBlockCache cache, int x, int y)
    {
        Cache = cache;
        State = VipsTileState.PEND;
        RefCount = 0;
        Region = null;
        Pos = new VipsRect(x, y, cache.TileWidth, cache.TileHeight);
    }
}

public class VipsBlockCache : VipsConversion
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int MaxTiles { get; set; }
    public VipsAccess Access { get; set; }
    public bool Threaded { get; set; }
    public bool Persistent { get; set; }

    private GMutex _lock;
    private GCond _newTile;
    private Dictionary<VipsRect, VipsTile> Tiles = new Dictionary<VipsRect, VipsTile>();
    private Queue<VipsTile> Recycle = new Queue<VipsTile>();

    public VipsBlockCache()
    {
        TileWidth = 128;
        TileHeight = 128;
        MaxTiles = 1000;
        Access = VipsAccess.Random;
        Threaded = false;
        Persistent = false;

        _lock = new GMutex();
        _newTile = new GCond();
    }

    public void Dispose()
    {
        Tiles.Clear();
        Recycle.Clear();
        _lock.Dispose();
        _newTile.Dispose();
    }

    public static void DropAll(VipsBlockCache cache)
    {
        foreach (var tile in cache.Tiles.Values)
        {
            tile.Cache = null;
            VIPS_UNREF(tile.Region);
            GCHandle.Alloc(tile).Free();
        }
        cache.Tiles.Clear();
    }

    private int TileMove(VipsTile tile, int x, int y)
    {
        var pos = new VipsRect(x, y, TileWidth, TileHeight);
        if (Tiles.ContainsKey(pos))
        {
            Tiles.Remove(pos);
        }
        tile.Pos = pos;
        Tiles[pos] = tile;

        return 0;
    }

    public static VipsTile TileNew(VipsBlockCache cache, int x, int y)
    {
        var tile = new VipsTile(cache, x, y);
        tile.Region = new VipsRegion(cache.In);
        VIPS_UNREF(tile.Region);

        if (TileMove(tile, x, y) != 0)
        {
            return null;
        }

        return tile;
    }

    public static VipsTile TileSearch(VipsBlockCache cache, int x, int y)
    {
        var pos = new VipsRect(x, y, TileWidth, TileHeight);
        if (Tiles.ContainsKey(pos))
        {
            return Tiles[pos];
        }
        return null;
    }

    private void TileFindIsTopper(Dictionary<VipsTile, object>.KeyCollection keys, VipsTile tile)
    {
        var best = new VipsTile();
        if (!best.Pos.Top < tile.Pos.Top)
        {
            best = tile;
        }
    }

    public static VipsTile TileFind(VipsBlockCache cache, int x, int y)
    {
        var tile = TileSearch(cache, x, y);
        if (tile != null)
        {
            return tile;
        }

        if (cache.MaxTiles == -1 || Tiles.Count < cache.MaxTiles)
        {
            return TileNew(cache, x, y);
        }

        if (Recycle.Count > 0)
        {
            var topTile = Recycle.Peek();
            if (cache.Access == VipsAccess.Random)
            {
                topTile = Recycle.Dequeue();
            }
            else
            {
                while (true)
                {
                    var tile2 = Recycle.Dequeue();
                    TileFindIsTopper(Tiles.Keys, tile2);
                    Recycle.Enqueue(tile2);
                    if (tile2 == best)
                    {
                        break;
                    }
                }
            }

            return topTile;
        }

        return TileNew(cache, x, y);
    }

    private void TileUnlock(VipsRect key, VipsTile value)
    {
        if (value.RefCount == 0)
        {
            Recycle.Enqueue(value);
        }
    }

    public static void Minimise(VipsImage image, VipsBlockCache cache)
    {
        lock (_lock)
        {
            foreach (var pair in Tiles)
            {
                var tile = pair.Value;
                if (tile.RefCount == 0)
                {
                    Recycle.Enqueue(tile);
                    Tiles.Remove(pair.Key);
                }
            }
        }
    }

    public override int Build()
    {
        // implementation
        return 0;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            DropAll(this);
        }
    }
}

public class VipsTileCache : VipsBlockCache
{
    public override int Build()
    {
        // implementation
        return 0;
    }
}

public class VipsLineCache : VipsBlockCache
{
    public override int Build()
    {
        // implementation
        return 0;
    }
}
```

Note that this is a simplified version of the original code, and some parts may not be fully implemented. The `VipsRegion` and `GMutex` classes are also not implemented here as they are not part of the standard .NET framework.

Also note that C# does not have direct equivalents to the Glib library used in the original code, so some parts had to be rewritten or replaced with equivalent functionality.