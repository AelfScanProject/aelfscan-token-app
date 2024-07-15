using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class TransferredProcessor : TokenProcessorBase<Transferred>
{
    public override async Task ProcessAsync(Transferred logEvent, LogEventContext context)
    {
        Logger.LogError($"start TransferredProcessor ProcessAsync:{context.Transaction.TransactionId}");
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        if (token == null)
        {
            Logger.LogError($"start TransferredProcessor ProcessAsync symbol:{logEvent.Symbol} token is null" );

        }

        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "Transfer";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        await AddTransferAsync(transfer, context);
        
        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.From.ToBase58(), -logEvent.Amount);
        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.To.ToBase58(), logEvent.Amount);
        
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.From.ToBase58(), logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.To.ToBase58(), logEvent.Symbol);
    }
}