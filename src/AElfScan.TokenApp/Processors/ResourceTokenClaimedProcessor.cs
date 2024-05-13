using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class ResourceTokenClaimedProcessor : TokenProcessorBase<ResourceTokenClaimed>
{
    public override async Task ProcessAsync(ResourceTokenClaimed logEvent, LogEventContext context)
    {
        if (logEvent.Payer != null)
        {
            await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.Payer.ToBase58(), -logEvent.Amount);
        }

        if (logEvent.Receiver != null)
        {
            await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.Receiver.ToBase58(), logEvent.Amount);
        }
    }
}