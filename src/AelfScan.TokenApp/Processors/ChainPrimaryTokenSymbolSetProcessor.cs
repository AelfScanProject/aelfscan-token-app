using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class ChainPrimaryTokenSymbolSetProcessor : TokenProcessorBase<ChainPrimaryTokenSymbolSet>
{
    public override async Task ProcessAsync(ChainPrimaryTokenSymbolSet logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.TokenSymbol);
        token.IsPrimaryToken = true;
        await SaveEntityAsync(token);
    }
}