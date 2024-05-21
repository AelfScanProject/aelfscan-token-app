using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class AccountToken : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string Address { get; set; }
    [Keyword] public string LowerCaseAddress { get; set; }
    public TokenBase Token { get; set; }
    public long Amount { get; set; }
    public decimal FormatAmount { get; set; }
    public long TransferCount { get; set; }
    public string FirstNftTransactionId { get; set; }
    public DateTime? FirstNftTime { get; set; }
}