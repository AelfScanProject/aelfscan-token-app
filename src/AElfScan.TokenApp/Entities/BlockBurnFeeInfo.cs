using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class BlockBurnFeeInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string Symbol { get; set; }
    public long Amount { get; set; }
    public long BlockHeight { get; set; }
}