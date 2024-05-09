using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class TransferInfo : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string TransactionId { get; set; }
    [Keyword] public string From { get; set; }
    [Keyword] public string To { get; set; }
    [Keyword] public string Method { get; set; }
    public long Amount { get; set; }
    public decimal FormatAmount { get; set; }
    public TokenBase Token { get; set; }
    [Keyword] public string Memo { get; set; }
    [Keyword] public string FromChainId { get; set; }
    [Keyword] public string ToChainId { get; set; }
    [Keyword] public string IssueChainId { get; set; }
    public long ParentChainHeight { get; set; }
    [Keyword] public string TransferTransactionId { get; set; }
    [Keyword] public string Status { get; set; }
    public Dictionary<string, string> ExtraProperties { get; set; }
}