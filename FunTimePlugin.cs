using System;

namespace FunTimeCobra;

public class FunTimeCorePlugin : IPlugin
{
    public string Name => "FunTimeCore";
    public string Version => "4.0.0";

    public void OnEnable(EventBus eventBus)
    {
        eventBus.OnBlockBreak += HandleBlockBreak;
    }

    private void HandleBlockBreak(BlockBreakEvent evt)
    {
        if (evt.BlockType == ItemType.Bedrock)
        {
            evt.IsCancelled = true;
        }
    }
}