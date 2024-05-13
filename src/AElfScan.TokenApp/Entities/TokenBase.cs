using AeFinder.Sdk.Entities;
using Nest;

namespace AElfScan.TokenApp.Entities;

public class TokenBase : AeFinderEntity
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string CollectionSymbol { get; set; }
    public SymbolType Type { get; set; }
    public int Decimals { get; set; }
}

public enum SymbolType
{
    Token,
    Nft,
    NftCollection
}