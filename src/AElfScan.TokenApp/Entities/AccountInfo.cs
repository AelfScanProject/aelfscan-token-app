using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class AccountInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string Address { get; set; }
    public long TokenHoldingCount { get; set; }
    public long TransferCount { get; set; }
}