using AeFinder.Sdk.Dtos;

namespace AElfScan.TokenApp.GraphQL;

public class TransferInfoDto : AeFinderEntityDto
{
    public string TransactionId { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public string Method { get; set; }
    public long Amount { get; set; } 
    public decimal FormatAmount { get; set; }
    public TokenBaseDto Token { get; set; }
    public string Memo { get; set; }
    public string FromChainId { get; set; }
    public string ToChainId { get; set; }
    public string IssueChainId { get; set; }
    public long ParentChainHeight { get; set; }
    public string TransferTransactionId { get; set; }
    public string Status { get; set; }
    public List<ExtraProperty> ExtraProperties { get; set; } = new();
}

public class ExtraProperty
{
    public string Key { get; set; }
    public string Value { get; set; }
}
 
public class TransferInfoPageResultDto
{
    public long TotalCount { get; set; }
    public List<TransferInfoDto> Items { get; set; }
}