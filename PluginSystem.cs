using System;
using System.Collections.Generic;

namespace FunTimeCobra;

public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    void OnEnable(EventBus eventBus);
}

public class BlockBreakEvent
{
    public Vector3Int Position { get; }
    public ItemType BlockType { get; }
    public bool IsCancelled { get; set; } = false;

    public BlockBreakEvent(Vector3Int position, ItemType blockType)
    {
        Position = position;
        BlockType = blockType;
    }
}

public class BlockPlaceEvent
{
    public Vector3Int Position { get; }
    public ItemType BlockType { get; set; }
    public bool IsCancelled { get; set; } = false;

    public BlockPlaceEvent(Vector3Int position, ItemType blockType)
    {
        Position = position;
        BlockType = blockType;
    }
}

public class EventBus
{
    public event Action<BlockBreakEvent>? OnBlockBreak;
    public event Action<BlockPlaceEvent>? OnBlockPlace;

    public bool TriggerBlockBreak(Vector3Int pos, ItemType type)
    {
        var evt = new BlockBreakEvent(pos, type);
        OnBlockBreak?.Invoke(evt);
        return !evt.IsCancelled;
    }

    public bool TriggerBlockPlace(Vector3Int pos, ref ItemType type)
    {
        var evt = new BlockPlaceEvent(pos, type);
        OnBlockPlace?.Invoke(evt);
        type = evt.BlockType;
        return !evt.IsCancelled;
    }
}

public class PluginManager
{
    public EventBus EventBus { get; } = new();
    private readonly List<IPlugin> _plugins = new();

    public void RegisterPlugin(IPlugin plugin)
    {
        _plugins.Add(plugin);
        plugin.OnEnable(EventBus);
    }
}