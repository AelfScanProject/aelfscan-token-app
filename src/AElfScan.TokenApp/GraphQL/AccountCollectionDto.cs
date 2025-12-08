using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class AccountCollectionDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public TokenBaseDto Token { get; set; }
    public decimal FormatAmount { get; set; }
    public long TransferCount { get; set; }
  
}

public class AccountCollectionPageResultDto
{
    public long TotalCount { get; set; }
    public List<AccountCollectionDto> Items { get; set; }
}