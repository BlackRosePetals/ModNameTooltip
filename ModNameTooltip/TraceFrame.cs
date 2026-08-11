using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;

namespace AssetPipelineTrace;

public sealed record DataTraceFrame(string? EditedBy, string? OnBehalfOf, IReadOnlySet<string> AddedKeys)
{
    public string ModId => OnBehalfOf ?? EditedBy ?? "UNKNOWN";
}
