using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp.GraphQL;

public class GetAccountTokenDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Symbol { get; set; }
    
    public string CollectionSymbol { get; set; }
    
    public string PartialSymbol { get; set; }
    public List<SymbolType> Types { get; set; } = new();
    
    public List<string> Symbols { get; set; } = new();
    
    public List<string> SearchSymbols { get; set; } = new();
    
    public string Search { get; set; } = "";
    
    //support txId, address, symbol
    public string FuzzySearch { get; set; } = "";
}