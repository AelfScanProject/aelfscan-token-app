namespace AElfScan.TokenApp.GraphQL;

public class GetAccountInfoDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
}