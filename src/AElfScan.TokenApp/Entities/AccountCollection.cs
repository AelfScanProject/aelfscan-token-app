using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class AccountCollection : AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string Address { get; set; }
    [Keyword] public string LowerCaseAddress { get; set; }
    public TokenBase Token { get; set; }
    public decimal FormatAmount { get; set; }
    public long TransferCount { get; set; }
  
}