using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class TransactionFeeChargedProcessor : TokenProcessorBase<TransactionFeeCharged>
{
    public override async Task ProcessAsync(TransactionFeeCharged logEvent, LogEventContext context)
    {
        if (logEvent.ChargingAddress != null)
        {
            await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.ChargingAddress.ToBase58(),
                -logEvent.Amount);
        }
    }
}