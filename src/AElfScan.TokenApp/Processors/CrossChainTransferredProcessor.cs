using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class CrossChainTransferredProcessor : TokenProcessorBase<CrossChainTransferred>
{
    public override async Task ProcessAsync(CrossChainTransferred logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "CrossChainTransfer";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        transfer.FromChainId = context.ChainId;
        await AddTransferAsync(transfer, context);
        
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.From.ToBase58(), logEvent.Symbol);
    }
}