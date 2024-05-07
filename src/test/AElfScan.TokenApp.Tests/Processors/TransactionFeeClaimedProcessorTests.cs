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
        var logEventContext = GenerateLogEventContext(@event);
        await _transactionFeeClaimedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();

        var accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = @event.Receiver.ToBase58(),
            Symbol = "ELF"
        });
        accountToken.Items[0].Amount.ShouldBe(1);
        accountToken.Items[0].FormatAmount.ShouldBe(0.00000001m);
    }
}