using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;
using Newtonsoft.Json;

namespace AElfScan.TokenApp.Processors;

public class IssuedProcessor : TokenProcessorBase<Issued>
{
    public override async Task ProcessAsync(Issued logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        token.Supply += logEvent.Amount;
        token.Issued += logEvent.Amount;
    
        await SaveEntityAsync(token);
        await ChangeCollectionItemCountAsync(context, token, logEvent.Amount);

        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "Issue";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        await AddTransferAsync(transfer, context);

        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.To.ToBase58(), logEvent.Amount);
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.To.ToBase58(), logEvent.Symbol);
    }
}