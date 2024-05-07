using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class AccountTokenDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public TokenBaseDto Token { get; set; }
    public long Amount { get; set; }
    public decimal FormatAmount { get; set; }
    public long TransferCount { get; set; }
    public string FirstNftTransactionId { get; set; }
    public DateTime? FirstNftTime { get; set; }
}

public class AccountTokenPageResultDto
{
    public long TotalCount { get; set; }
    public List<AccountTokenDto> Items { get; set; }
}