using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class RentalChargedProcessor : TokenProcessorBase<RentalCharged>
{
    public override async Task ProcessAsync(RentalCharged logEvent, LogEventContext context)
    {
        Logger.LogInformation("RentalChargedProcessor start");
        
        if (logEvent.Amount == 0)
        {
            return;
        }

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