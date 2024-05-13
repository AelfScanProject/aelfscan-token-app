using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class TransactionFeeClaimedProcessor : TokenProcessorBase<TransactionFeeClaimed>
{
    public override async Task ProcessAsync(TransactionFeeClaimed logEvent, LogEventContext context)
    {
        if (logEvent.Receiver != null)
        {
            await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.Receiver.ToBase58(), logEvent.Amount);
        }
    }
}