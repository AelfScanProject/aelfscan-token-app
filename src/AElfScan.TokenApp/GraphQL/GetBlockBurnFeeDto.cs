using JetBrains.Annotations;

namespace AElfScan.TokenApp.GraphQL;

public class GetBlockBurnFeeDto
{
    [NotNull] public string  ChainId  { get; set; }
    public long  BeginBlockHeight  { get; set; }
    public long  EndBlockHeight  { get; set; }
}