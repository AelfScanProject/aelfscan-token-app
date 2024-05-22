using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TransactionFeeChargedProcessorTests: TokenContractAppTestBase
{
    private readonly TransactionFeeChargedProcessor _transactionFeeChargedProcessor;

    public TransactionFeeChargedProcessorTests()
    {
        _transactionFeeChargedProcessor = GetRequiredService<TransactionFeeChargedProcessor>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await CreateTokenAsync();
        
        var @event = new TransactionFeeCharged
        {
            Amount = 1,
            Symbol = "ELF",
            ChargingAddress = TestAddress
        };
        
        //before
        var accountTokenBefore = await GetAccountTokenAsync(ChainId, TestAddress.ToBase58(), @event.Symbol);
        
        var logEventContext = GenerateLogEventContext(@event);
        await _transactionFeeChargedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        //check
        var accountToken = await GetAccountTokenAsync(ChainId, TestAddress.ToBase58(), @event.Symbol);
        (accountTokenBefore.Amount - accountToken.Amount).ShouldBe(1);
        (accountTokenBefore.FormatAmount - accountToken.FormatAmount).ShouldBe(0.00000001m);
    }
}