using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class IssuedAndBurnedProcessorTests : TokenContractAppTestBase
{
    private readonly BurnedProcessor _burnedProcessor;
    
    public IssuedAndBurnedProcessorTests()
    {
        _burnedProcessor = GetRequiredService<BurnedProcessor>();
        TokenAppConstants.InitialBalanceEndHeight = new Dictionary<string, long>
        {
            { "AELF", 99 }
        };
    }

    [Fact]
    public async Task HandleEventAsync_Test()
    {
        await CreateTokenAsync();
        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbols = new List<string>() { "ELF"}
        });
        token.Items[0].Supply.ShouldBe(100);
        token.Items[0].Issued.ShouldBe(100);
        token.Items[0].HolderCount.ShouldBe(1);
        token.Items[0].TransferCount.ShouldBe(1);
        
        var account = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        account[0].TransferCount.ShouldBe(1);
        account[0].TokenHoldingCount.ShouldBe(1);
        
        var accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        accountToken.Items[0].Amount.ShouldBe(100);
        accountToken.Items[0].FormatAmount.ShouldBe(0.000001m);
        accountToken.Items[0].TransferCount.ShouldBe(1);
        
        var transfer = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        transfer.Items.Count.ShouldBe(1);
        transfer.Items[0].TransactionId.ShouldBe(TransactionId);
        transfer.Items[0].From.ShouldBeNull();
        transfer.Items[0].To.ShouldBe(TestAddress.ToBase58());
        transfer.Items[0].Method.ShouldBe("Issue");
        transfer.Items[0].Amount.ShouldBe(100);
        transfer.Items[0].FormatAmount.ShouldBe(0.000001m);
        transfer.Items[0].Token.Symbol.ShouldBe("ELF");
        transfer.Items[0].Memo.ShouldBe("memo");
        transfer.Items[0].FromChainId.ShouldBeNull();
        transfer.Items[0].ToChainId.ShouldBeNull();
        transfer.Items[0].IssueChainId.ShouldBeNull();
        transfer.Items[0].ParentChainHeight.ShouldBe(0);
        transfer.Items[0].TransferTransactionId.ShouldBeNull();
        var dailyHolder = await   Query.DailyHolder(DailyHolderInfoReadOnlyRepository, ObjectMapper, new GetDailyHolderDto
        {
            ChainId = ChainId
        });
        dailyHolder.Count.ShouldBeGreaterThan(0);
        var burned = new Burned
        {
            Amount = 10,
            Symbol = "ELF",
            Burner = TestAddress
        };
        var logEventContext = GenerateLogEventContext(burned);
        await _burnedProcessor.ProcessAsync(logEventContext);

        token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = logEventContext.ChainId,
            Symbols = new List<string>() {burned.Symbol}
        });
        token.Items[0].Supply.ShouldBe(90);
        token.Items[0].Issued.ShouldBe(100);
        token.Items[0].HolderCount.ShouldBe(1);
        token.Items[0].TransferCount.ShouldBe(2);
        
        account = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        account[0].TransferCount.ShouldBe(2);
        account[0].TokenHoldingCount.ShouldBe(1);

        accountToken = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        accountToken.Items[0].Amount.ShouldBe(90);
        accountToken.Items[0].FormatAmount.ShouldBe(0.0000009m);
        accountToken.Items[0].TransferCount.ShouldBe(2);
        
        transfer = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        transfer.Items.Count.ShouldBe(2);
        transfer.Items[1].TransactionId.ShouldBe(TransactionId);
        transfer.Items[1].From.ShouldBe(TestAddress.ToBase58());
        transfer.Items[1].To.ShouldBeNull();
        transfer.Items[1].Method.ShouldBe("Burn");
        transfer.Items[1].Amount.ShouldBe(10);
        transfer.Items[1].FormatAmount.ShouldBe(0.0000001m);
        transfer.Items[1].Token.Symbol.ShouldBe("ELF");
        transfer.Items[1].Memo.ShouldBeNull();
        transfer.Items[1].FromChainId.ShouldBeNull();
        transfer.Items[1].ToChainId.ShouldBeNull();
        transfer.Items[1].IssueChainId.ShouldBeNull();
        transfer.Items[1].ParentChainHeight.ShouldBe(0);
        transfer.Items[1].TransferTransactionId.ShouldBeNull();
        
        var blockBurnFeeListDto = await Query.BlockBurnFeeInfo(BlockBurnFeeInfoReadOnlyRepository, ObjectMapper, new GetBlockBurnFeeDto()
        {
            ChainId = logEventContext.ChainId,
            BeginBlockHeight =BlockHeight,
            EndBlockHeight = BlockHeight + 1
        });
        blockBurnFeeListDto.Items.Count.ShouldBe(1);
    }
}