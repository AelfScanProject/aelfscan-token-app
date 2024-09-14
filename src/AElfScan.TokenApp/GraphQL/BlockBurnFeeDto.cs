using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class BlockBurnFeeDto :AeFinderEntityDto
{
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public long BlockHeight { get; set; }
}

public class BlockBurnFeeListDto
{
    public List<BlockBurnFeeDto> Items { get; set; }
}