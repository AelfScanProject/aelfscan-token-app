using AeFinder.Sdk.Dtos;
using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp.GraphQL;

public class TokenBaseDto : AeFinderEntityDto
{
    public string Symbol { get; set; }
    public string CollectionSymbol { get; set; }
    public SymbolType Type { get; set; }
    public int Decimals { get; set; }
}