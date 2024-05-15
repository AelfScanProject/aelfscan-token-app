namespace AElfScan.TokenApp.GraphQL;

public class GetAccountInfoDto : PagedResultQueryDto
{
    public string ChainId { get; set; }
    public string Address { get; set; }
}


public class GetAccountCountDto 
{
    public string ChainId { get; set; }
}

