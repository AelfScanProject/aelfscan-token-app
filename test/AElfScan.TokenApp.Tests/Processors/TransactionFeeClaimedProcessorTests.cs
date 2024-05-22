using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TransactionFeeClaimedProcessorTests: TokenContractAppTestBase
{
    private readonly TransactionFeeClaimedProcessor _transactionFeeClaimedProcessor;

    public TransactionFeeClaimedProcessorTests()
    {
        _transactionFeeClaimedProcessor = GetRequiredService<TransactionFeeClaimedProcessor>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await CreateTokenAsync();
        
        var @event = new TransactionFeeClaimed
        {
            Amount = 1,
            Symbol = "ELF",
            Receiver = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG")
        };
        //before
        var accountTokenBefore = await GetAccountTokenAsync(ChainId, @event.Receiver.ToBase58(), @event.Symbol);

        var logEventContext = GenerateLogEventContext(@event);
        await _transactionFeeClaimedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var accountToken = await GetAccountTokenAsync(ChainId, @event.Receiver.ToBase58(), @event.Symbol);
        
        //check
        (accountToken.Amount - (accountTokenBefore?.Amount ?? 0)).ShouldBe(1);
        (accountToken.FormatAmount - (accountTokenBefore?.FormatAmount ?? 0)).ShouldBe(0.00000001m);
    }
}