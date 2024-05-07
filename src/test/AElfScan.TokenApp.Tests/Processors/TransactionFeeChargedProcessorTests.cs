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
        var logEventContext = GenerateLogEventContext(@event);
        await _transactionFeeChargedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();

        var accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        accountToken.Items[0].Amount.ShouldBe(99);
        accountToken.Items[0].FormatAmount.ShouldBe(0.00000099m);
    }
}