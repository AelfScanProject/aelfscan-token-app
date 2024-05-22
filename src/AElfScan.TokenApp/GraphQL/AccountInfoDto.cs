using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class AccountInfoDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public long TokenHoldingCount { get; set; }
    public long TransferCount { get; set; }
}

public class AccountCountDto
{
    public int Count { get; set; }
}

public class AccountInfoPageResultDto
{
    public long TotalCount { get; set; }
    public List<AccountInfoDto> Items { get; set; }
}