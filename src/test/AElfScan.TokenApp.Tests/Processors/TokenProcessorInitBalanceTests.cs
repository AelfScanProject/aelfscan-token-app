using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TokenProcessorInitBalanceTests : TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;
    public TokenProcessorInitBalanceTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
        
        TokenAppConstants.StartProcessBalanceEventHeight = new Dictionary<string, long>
        {
            { "AELF", 1000 }
        };
    }

    [Fact]
    public async Task ModifyBalanceTest()
    {
        await CreateCollectionTokenAsync();
        await CreateNftTokenAsync();

        var collectionSymbol = "NFT-0";
        var transferred = new Transferred
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "NFT-1",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(transferred);
        await _transferredProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var tokenNftPage = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = transferred.Symbol
        });
        var tokenNft = tokenNftPage.Items;
        tokenNft[0].HolderCount.ShouldBe(0);
        tokenNft[0].TransferCount.ShouldBe(2);
        
        var tokenCollection = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = collectionSymbol
        });
        tokenCollection.Items[0].HolderCount.ShouldBe(0);
        tokenCollection.Items[0].TransferCount.ShouldBe(3);
        
        var accountFrom = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        accountFrom[0].TransferCount.ShouldBe(3);
        accountFrom[0].TokenHoldingCount.ShouldBe(0);
        
        var accountTo = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        accountTo[0].TransferCount.ShouldBe(1);
        accountTo[0].TokenHoldingCount.ShouldBe(0);
        
        var accountNftTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountNftTokenFrom.Items[0].TransferCount.ShouldBe(2);
        accountNftTokenFrom.Items[0].FirstNftTransactionId.ShouldBe(TransactionId);
        accountNftTokenFrom.Items[0].FirstNftTime.ShouldNotBeNull();
        accountNftTokenFrom.Items[0].Amount.ShouldBe(0);
        accountNftTokenFrom.Items[0].FormatAmount.ShouldBe(0);
        
        var accountCollectionTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionTokenFrom.Items[0].TransferCount.ShouldBe(3);
        accountCollectionTokenFrom.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountCollectionTokenFrom.Items[0].FirstNftTime.ShouldBeNull();
        accountCollectionTokenFrom.Items[0].Amount.ShouldBe(0);
        accountCollectionTokenFrom.Items[0].FormatAmount.ShouldBe(0);

        var accountNftTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountNftTokenTo.Items[0].TransferCount.ShouldBe(1);
        accountNftTokenTo.Items[0].FirstNftTransactionId.ShouldBe(TransactionId);
        accountNftTokenTo.Items[0].FirstNftTime.ShouldBe(logEventContext.Block.BlockTime);
        accountNftTokenTo.Items[0].Amount.ShouldBe(0);
        accountNftTokenTo.Items[0].FormatAmount.ShouldBe(0);
        
        var accountCollectionTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionTokenTo.Items[0].TransferCount.ShouldBe(1);
        accountCollectionTokenTo.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountCollectionTokenTo.Items[0].FirstNftTime.ShouldBeNull();
        accountCollectionTokenTo.Items[0].Amount.ShouldBe(0);
        accountCollectionTokenTo.Items[0].FormatAmount.ShouldBe(0);
    }
}