using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class TokenInfo : TokenBase, IAeFinderEntity
{
    [Keyword] public string TokenName { get; set; }
    [Keyword] public string LowerCaseTokenName { get; set; }
    public long TotalSupply { get; set; }
    public long Supply { get; set; }
    public long Issued { get; set; }
    [Keyword] public string Issuer { get; set; }
    [Keyword] public string Owner { get; set; }
    public bool IsPrimaryToken { get; set; }
    public bool IsBurnable { get; set; }
    [Keyword] public string IssueChainId { get; set; }
    public Dictionary<string, string> ExternalInfo { get; set; } = new();
    public long HolderCount { get; set; }
    public long TransferCount { get; set; }
}