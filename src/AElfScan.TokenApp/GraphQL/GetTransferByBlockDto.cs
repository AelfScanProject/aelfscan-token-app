
namespace AElfScan.TokenApp.GraphQL;

public class GetTransferByBlockDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public List<string> FromList { get; set; }
    public List<string> ToList { get; set; }
    public List<string> Methods { get; set; } = new();
    public List<string> SymbolList { get; set; }
    public long BeginBlockHeight { get; set; }
    public long? EndBlockHeight { get; set; }
}