using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class CrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    public override async Task ProcessAsync(CrossChainReceived logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        token.Supply += logEvent.Amount;
        await SaveEntityAsync(token);
        
        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "CrossChainReceive";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        transfer.ToChainId = context.ChainId;
        await AddTransferAsync(transfer, context);

        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.To.ToBase58(), logEvent.Amount);
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.To.ToBase58(), logEvent.Symbol);
    }
}