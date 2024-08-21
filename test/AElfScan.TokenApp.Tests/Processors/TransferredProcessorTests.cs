using AElfScan.TokenApp.GraphQL;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TransferredProcessorTests: TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;

    public TransferredProcessorTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
    }

    [Fact]
    public async Task HandleEvent_Test()
    {
        await CreateTokenAsync();
        

        var transferred = new Transferred
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(transferred);
        await _transferredProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = "ELF"
        });
        token.Items[0].HolderCount.ShouldBe(2);
        token.Items[0].TransferCount.ShouldBe(2);
        
        var accountFrom = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        accountFrom[0].TransferCount.ShouldBe(2);
        accountFrom[0].TokenHoldingCount.ShouldBe(1);
        
        var accountTo = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        accountTo[0].TransferCount.ShouldBe(1);
        accountTo[0].TokenHoldingCount.ShouldBe(1);
        
        var accountTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        accountTokenFrom.Items[0].Amount.ShouldBe(99);
        accountTokenFrom.Items[0].FormatAmount.ShouldBe(0.00000099m);
        accountTokenFrom.Items[0].TransferCount.ShouldBe(2);
        accountTokenFrom.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountTokenFrom.Items[0].FirstNftTime.ShouldBeNull();
        
        var accountTokenToPage = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = "ELF"
        });
        var accountTokenTo = accountTokenToPage.Items;
        accountTokenTo[0].Amount.ShouldBe(1);
        accountTokenTo[0].FormatAmount.ShouldBe(0.00000001m);
        accountTokenTo[0].TransferCount.ShouldBe(1);
        accountTokenTo[0].FirstNftTransactionId.ShouldBeNull();
        accountTokenTo[0].FirstNftTime.ShouldBeNull();
        
        var transferFromPage = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        var transferFrom = transferFromPage.Items;
        transferFrom.Count.ShouldBe(2);
        transferFrom[1].TransactionId.ShouldBe(TransactionId);
        transferFrom[1].From.ShouldBe(transferred.From.ToBase58());
        transferFrom[1].To.ShouldBe(transferred.To.ToBase58());
        transferFrom[1].Method.ShouldBe("Transfer");
        transferFrom[1].Amount.ShouldBe(transferred.Amount);
        transferFrom[1].FormatAmount.ShouldBe(0.00000001m);
        transferFrom[1].Token.Symbol.ShouldBe(transferred.Symbol);
        transferFrom[1].Memo.ShouldBe(transferred.Memo);
        transferFrom[1].FromChainId.ShouldBeNull();
        transferFrom[1].ToChainId.ShouldBeNull();
        transferFrom[1].IssueChainId.ShouldBeNull();
        transferFrom[1].ParentChainHeight.ShouldBe(0);
        transferFrom[1].TransferTransactionId.ShouldBeNull();
        
        var transferToPage = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        var transferTo = transferToPage.Items;
        transferTo.Count.ShouldBe(1);
        transferTo[0].TransactionId.ShouldBe(TransactionId);
        transferTo[0].From.ShouldBe(transferred.From.ToBase58());
        transferTo[0].To.ShouldBe(transferred.To.ToBase58());
        transferTo[0].Method.ShouldBe("Transfer");
        transferTo[0].Amount.ShouldBe(transferred.Amount);
        transferTo[0].FormatAmount.ShouldBe(0.00000001m);
        transferTo[0].Token.Symbol.ShouldBe(transferred.Symbol);
        transferTo[0].Memo.ShouldBe(transferred.Memo);
        transferTo[0].FromChainId.ShouldBeNull();
        transferTo[0].ToChainId.ShouldBeNull();
        transferTo[0].IssueChainId.ShouldBeNull();
        transferTo[0].ParentChainHeight.ShouldBe(0);
        transferTo[0].TransferTransactionId.ShouldBeNull();
    }
    
    [Fact]
    public async Task HandleEvent_Collection_Test()
    {
        await CreateCollectionTokenAsync();
        var transferred = new Transferred
        {
            Amount = 1,
            From = TestAddress,
            Symbol = "NFT-0",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(transferred);
        await _transferredProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = transferred.Symbol
        });
        token.Items[0].HolderCount.ShouldBe(0);
        token.Items[0].TransferCount.ShouldBe(0);
        token.Items[0].ItemCount.ShouldBe(0);
        
        var accountFrom = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        accountFrom[0].TransferCount.ShouldBe(2);
        accountFrom[0].TokenHoldingCount.ShouldBe(0);
        
        var accountTo = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        accountTo[0].TransferCount.ShouldBe(1);
        accountTo[0].TokenHoldingCount.ShouldBe(0);
        
        var accountTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountTokenFrom.Items[0].TransferCount.ShouldBe(2);
        accountTokenFrom.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountTokenFrom.Items[0].FirstNftTime.ShouldBeNull();
        
        var accountTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountTokenTo.Items[0].TransferCount.ShouldBe(1);
        accountTokenFrom.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountTokenFrom.Items[0].FirstNftTime.ShouldBeNull();
        
        var accountCollectionFrom = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountCollectionFrom.Items.ShouldBeEmpty();
        
        var accountCollectionTo = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountCollectionTo.Items.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task HandleEvent_Nft_Test()
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
        
        var tokenNft = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = transferred.Symbol
        });
        tokenNft.Items[0].HolderCount.ShouldBe(2);
        tokenNft.Items[0].TransferCount.ShouldBe(2);
        
        var tokenCollection = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = collectionSymbol
        });
        tokenCollection.Items[0].HolderCount.ShouldBe(2);
        tokenCollection.Items[0].TransferCount.ShouldBe(2);
        
        var accountFrom = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        accountFrom[0].TransferCount.ShouldBe(3);
        accountFrom[0].TokenHoldingCount.ShouldBe(1);
        
        var accountTo = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        accountTo[0].TransferCount.ShouldBe(1);
        accountTo[0].TokenHoldingCount.ShouldBe(1);
        
        var accountNftTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountNftTokenFrom.Items[0].TransferCount.ShouldBe(2);
        accountNftTokenFrom.Items[0].FirstNftTransactionId.ShouldBe(TransactionId);
        accountNftTokenFrom.Items[0].FirstNftTime.ShouldNotBeNull();
        
        var accountCollectionTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionTokenFrom.Items[0].TransferCount.ShouldBe(1);
        accountCollectionTokenFrom.Items[0].FirstNftTransactionId.ShouldBeNull();
        accountCollectionTokenFrom.Items[0].FirstNftTime.ShouldBeNull();

        var accountNftTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountNftTokenTo.Items[0].TransferCount.ShouldBe(1);
        accountNftTokenTo.Items[0].FirstNftTransactionId.ShouldBe(TransactionId);
        accountNftTokenTo.Items[0].FirstNftTime.ShouldBe(logEventContext.Block.BlockTime);
        
        var accountCollectionTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionTokenTo.Items.ShouldBeEmpty();
        
        var accountCollectionFrom1 = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountCollectionFrom1.Items.ShouldBeEmpty();
        
        var accountCollectionTo1 = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = transferred.Symbol
        });
        accountCollectionTo1.Items.ShouldBeEmpty();

        var accountCollectionFrom2 = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionFrom2.Items[0].TransferCount.ShouldBe(2);
        accountCollectionFrom2.Items[0].FormatAmount.ShouldBe(99);
        
        var accountCollectionTo2 = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = collectionSymbol
        });
        accountCollectionTo2.Items[0].TransferCount.ShouldBe(1);
        accountCollectionTo2.Items[0].FormatAmount.ShouldBe(1);

    }
    
    [Fact]
    public async Task HandleEvent_BalanceTo0_Test()
    {
        await CreateTokenAsync();
        

        var transferred = new Transferred
        {
            Amount = 100,
            From = TestAddress,
            Symbol = "ELF",
            To = Address.FromBase58("2XDRhxzMbaYRCTe3NxRpARkBpjfQpyWdBKscQpc3Tph3m6dqHG"),
            Memo = "memo"
        };
        var logEventContext = GenerateLogEventContext(transferred);
        await _transferredProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
        
        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbol = "ELF"
        });
        token.Items[0].HolderCount.ShouldBe(1);
        token.Items[0].TransferCount.ShouldBe(2);
        
        var accountFrom = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58()
        });
        accountFrom[0].TransferCount.ShouldBe(2);
        accountFrom[0].TokenHoldingCount.ShouldBe(0);
        
        var accountTo = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58()
        });
        accountTo[0].TransferCount.ShouldBe(1);
        accountTo[0].TokenHoldingCount.ShouldBe(1);
        
        var accountTokenFrom = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = TestAddress.ToBase58(),
            Symbol = "ELF"
        });
        accountTokenFrom.Items.ShouldBeEmpty();
        
        var accountTokenTo = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            Address = transferred.To.ToBase58(),
            Symbol = "ELF"
        });
        accountTokenTo.Items[0].Amount.ShouldBe(100);
        accountTokenTo.Items[0].FormatAmount.ShouldBe(0.000001m);
        accountTokenTo.Items[0].TransferCount.ShouldBe(1);
    }
}