using AElfScan.TokenApp.GraphQL;
using AElf;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class CrossChainTransferredProcessorTests: TokenContractAppTestBase
{
    private readonly CrossChainTransferredProcessor _crossChainTransferredProcessor;

    public CrossChainTransferredProcessorTests()
    {
        _crossChainTransferredProcessor = GetRequiredService<CrossChainTransferredProcessor>();
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await CreateTokenAsync();
        

        var @event = new CrossChainTransferred
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo",
            IssueChainId = 1,
            ToChainId = 2
        };
        var logEventContext = GenerateLogEventContext(@event);
        await _crossChainTransferredProcessor.ProcessAsync(logEventContext);
        
        var transfer = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        transfer.Items.Count.ShouldBe(2);
        transfer.Items[1].TransactionId.ShouldBe(TransactionId);
        transfer.Items[1].From.ShouldBe(TestAddress.ToBase58());
        transfer.Items[1].To.ShouldBe(@event.To.ToBase58());
        transfer.Items[1].Method.ShouldBe("CrossChainTransfer");
        transfer.Items[1].Amount.ShouldBe(1);
        transfer.Items[1].FormatAmount.ShouldBe((decimal)0.00000001);
        transfer.Items[1].Token.Symbol.ShouldBe(@event.Symbol);
        transfer.Items[1].Memo.ShouldBe(@event.Memo);
        transfer.Items[1].FromChainId.ShouldBe(logEventContext.ChainId);
        transfer.Items[1].ToChainId.ShouldBe(ChainHelper.ConvertChainIdToBase58(@event.ToChainId));
        transfer.Items[1].IssueChainId.ShouldBe(ChainHelper.ConvertChainIdToBase58(@event.IssueChainId));
        transfer.Items[1].ParentChainHeight.ShouldBe(0);
        transfer.Items[1].TransferTransactionId.ShouldBeNull();
        
        var account = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        account[0].TransferCount.ShouldBe(2);
        account[0].TokenHoldingCount.ShouldBe(1);

        var accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = @event.Symbol
        });
        accountToken.Items[0].TransferCount.ShouldBe(2);
        accountToken.Items[0].Token.Symbol.ShouldBe(@event.Symbol);
    }
}