using AeFinder.Entities;
using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class DailyHolderInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string DateStr { get; set; }

    public long Count { get; set; }

    [Keyword] public string ChainId { get; set; }
}