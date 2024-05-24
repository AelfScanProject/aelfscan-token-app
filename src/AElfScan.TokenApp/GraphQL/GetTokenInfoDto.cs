using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp.GraphQL;

public class GetTokenInfoDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    
    public string Symbol { get; set; }
    public List<string> Symbols { get; set; } = new();
    public string PartialSymbol { get; set; }
    public string TokenName { get; set; }
    public string PartialTokenName { get; set; }
    public string Owner { get; set; }
    public string Issuer { get; set; }
    public List<SymbolType> Types { get; set; } = new();

    public List<string> CollectionSymbols { get; set; } = new();
    
    public string Search { get; set; } = "";
    
    public string ExactSearch { get; set; } = "";
    
    public string FuzzySearch { get; set; } = "";
}