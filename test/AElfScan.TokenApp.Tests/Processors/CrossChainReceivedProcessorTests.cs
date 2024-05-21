using AElfScan.TokenApp.GraphQL;
using AElf;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class CrossChainReceivedProcessorTests : TokenContractAppTestBase
{
    private readonly CrossChainReceivedProcessor _crossChainReceivedProcessor;

    public CrossChainReceivedProcessorTests()
    {
        _crossChainReceivedProcessor = GetRequiredService<CrossChainReceivedProcessor>();
    }

    [Fact]
    public async Task HandleEvent_Test()
    {
        await CreateTokenAsync();
        
        var @event = new CrossChainReceived
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo",
            IssueChainId = 1,
            FromChainId = 1,
            ParentChainHeight = 100,
            TransferTransactionId = Hash.LoadFromHex("cd29ff43ce541c76752638cbc67ce8d4723fd5142cacffa36a95a40c93d30a4c")
        };
        var logEventContext = GenerateLogEventContext(@event);
        await _crossChainReceivedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();

        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbols = new List<string>() { @event.Symbol }
        });
        token.Items[0].Supply.ShouldBe(101);
        token.Items[0].TransferCount.ShouldBe(2);
        token.Items[0].HolderCount.ShouldBe(2);

        var transfer = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = @event.To.ToBase58()
        });
        transfer.Items.Count.ShouldBe(1);
        transfer.Items[0].TransactionId.ShouldBe(TransactionId);
        transfer.Items[0].From.ShouldBe(TestAddress.ToBase58());
        transfer.Items[0].To.ShouldBe(@event.To.ToBase58());
        transfer.Items[0].Method.ShouldBe("CrossChainReceive");
        transfer.Items[0].Amount.ShouldBe(1);
        transfer.Items[0].FormatAmount.ShouldBe((decimal)0.00000001);
        transfer.Items[0].Token.Symbol.ShouldBe(@event.Symbol);
        transfer.Items[0].Memo.ShouldBe(@event.Memo);
        transfer.Items[0].FromChainId.ShouldBe(ChainHelper.ConvertChainIdToBase58(@event.FromChainId));
        transfer.Items[0].ToChainId.ShouldBe(logEventContext.ChainId);
        transfer.Items[0].IssueChainId.ShouldBe(ChainHelper.ConvertChainIdToBase58(@event.IssueChainId));
        transfer.Items[0].ParentChainHeight.ShouldBe(@event.ParentChainHeight);
        transfer.Items[0].TransferTransactionId.ShouldBe(@event.TransferTransactionId.ToHex());
        
        var count = await Query.AccountCount(AccountInfoReadOnlyRepository, new GetAccountCountDto()
        {
            ChainId = ChainId,
        });
        
        count.Count.ShouldBe(2);
        
        var account = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = @event.To.ToBase58()
        });
        account[0].TransferCount.ShouldBe(1);
        account[0].TokenHoldingCount.ShouldBe(1);

        var accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper, new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = @event.To.ToBase58(),
            Symbol = @event.Symbol
        });
        accountToken.Items[0].Amount.ShouldBe(1);
        accountToken.Items[0].FormatAmount.ShouldBe((decimal)0.00000001);
        accountToken.Items[0].TransferCount.ShouldBe(1);
        accountToken.Items[0].Token.Symbol.ShouldBe(@event.Symbol);
    }
}