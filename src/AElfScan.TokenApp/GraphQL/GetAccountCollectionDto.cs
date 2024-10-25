using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp.GraphQL;

public class GetAccountCollectionDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
    public string Symbol { get; set; }
    
    public List<string> AddressList { get; set; } = new();

}