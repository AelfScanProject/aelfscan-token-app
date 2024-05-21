using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class RentalChargedProcessorTests: TokenContractAppTestBase
{
    private readonly RentalChargedProcessor _rentalChargedProcessor;

    public RentalChargedProcessorTests()
    {
        _rentalChargedProcessor = GetRequiredService<RentalChargedProcessor>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await CreateTokenAsync();
        
        
        var @event = new RentalCharged
        {
            Amount = 1,
            Symbol = "ELF",
            Payer = TestAddress,
            Receiver = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG")
        };
        var logEventContext = GenerateLogEventContext(@event);
        await _rentalChargedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var balancePayer = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        balancePayer.Items[0].Amount.ShouldBe(99);
        balancePayer.Items[0].FormatAmount.ShouldBe(0.00000099m);

        var balanceReceiver = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = @event.Receiver.ToBase58(),
            Symbol = "ELF"
        });
        balanceReceiver.Items[0].Amount.ShouldBe(1);
        balanceReceiver.Items[0].FormatAmount.ShouldBe(0.00000001m);
    }
}