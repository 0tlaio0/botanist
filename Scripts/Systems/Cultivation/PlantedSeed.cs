using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace Botanist.Scripts;

public sealed class PlantedSeed
{
    public required IBotanistSeedCard Seed { get; init; }
    public CardModel Card => Seed.Card;
    public Dictionary<BotanistElement, int> Remaining { get; } = new();

    public bool IsRipe => Remaining.Values.All(count => count <= 0);
}
