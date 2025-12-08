using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class GetDailyHolderDto
{
    public string ChainId { get; set; }
}

public class DailyHolderDto
{
    public string DateStr { get; set; }
    public long Count { get; set; }
}