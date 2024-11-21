using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.GraphQL;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TokenBalanceInitBlockProcessorTests : TokenContractAppTestBase
{
    private readonly IBlockProcessor _blockDataProcessor;

    public TokenBalanceInitBlockProcessorTests()
    {
        _blockDataProcessor = GetRequiredService<IBlockProcessor>();
        TokenAppConstants.InitialBalanceEndHeight = new Dictionary<string, long>
        {
            { "AELF", 99 }
        };
    }

    [Fact]
    public async Task Handle_Test()
    {
        await _blockDataProcessor.ProcessAsync(
            new Block
            {
                BlockHeight = 100
            },
            new BlockContext { ChainId = "AELF" });
        
        var accountTokens = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        accountTokens.Items.Count.ShouldBe(0);
        
        await _blockDataProcessor.ProcessAsync(
            new Block
            {
                BlockHeight = 99
            },
            new BlockContext { ChainId = "AELF" });
        
        var tokens = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper,new GetTokenInfoDto
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        tokens.Items.Count.ShouldBe(4);
        tokens.Items.First(o => o.Symbol=="ELF").HolderCount.ShouldBe(2);
        tokens.Items.First(o => o.Symbol=="NFT-0").HolderCount.ShouldBe(2);
        tokens.Items.First(o => o.Symbol=="NFT-1").HolderCount.ShouldBe(1);
        tokens.Items.First(o => o.Symbol=="NFT-2").HolderCount.ShouldBe(1);

        accountTokens = await Query.AccountToken(AccountTokenReadOnlyRepository, ObjectMapper,new GetAccountTokenDto
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        accountTokens.Items.Count.ShouldBe(5);
        var  accountToken1 = accountTokens.Items.Where(o => o.Address=="Address1").ToList();
        accountToken1.Count.ShouldBe(2);
        
        var accountToken = accountToken1.First(o => o.Token.Symbol=="ELF");
        accountToken.Token.Decimals.ShouldBe(8);
        accountToken.Token.Symbol.ShouldBe("ELF");
        accountToken.Token.Type.ShouldBe( SymbolType.Token);
        accountToken.Token.CollectionSymbol.ShouldBeNull();
        accountToken.Amount.ShouldBe(100000000);
        accountToken.FormatAmount.ShouldBe(1);
        
        accountToken = accountToken1.First(o => o.Token.Symbol=="NFT-2");
        accountToken.Token.Decimals.ShouldBe(0);
        accountToken.Token.Symbol.ShouldBe("NFT-2");
        accountToken.Token.Type.ShouldBe( SymbolType.Nft);
        accountToken.Token.CollectionSymbol.ShouldBe("NFT-0");
        accountToken.Amount.ShouldBe(300);
        accountToken.FormatAmount.ShouldBe(300);
        
        accountToken = accountTokens.Items.First(o => o.Address=="Address2");
        accountToken.Token.Decimals.ShouldBe(8);
        accountToken.Token.Symbol.ShouldBe("ELF");
        accountToken.Token.Type.ShouldBe( SymbolType.Token);
        accountToken.Token.CollectionSymbol.ShouldBeNull();
        accountToken.Amount.ShouldBe(200000000);
        accountToken.FormatAmount.ShouldBe(2);
        
        accountToken = accountTokens.Items.First(o => o.Address=="Address3");
        accountToken.Token.Decimals.ShouldBe(0);
        accountToken.Token.Symbol.ShouldBe("NFT-0");
        accountToken.Token.Type.ShouldBe( SymbolType.NftCollection);
        accountToken.Token.CollectionSymbol.ShouldBeNull();
        accountToken.Amount.ShouldBe(1);
        accountToken.FormatAmount.ShouldBe(1);
        
        accountToken = accountTokens.Items.First(o => o.Address=="Address4");
        accountToken.Token.Decimals.ShouldBe(0);
        accountToken.Token.Symbol.ShouldBe("NFT-1");
        accountToken.Token.Type.ShouldBe( SymbolType.Nft);
        accountToken.Token.CollectionSymbol.ShouldBe("NFT-0");
        accountToken.Amount.ShouldBe(200);
        accountToken.FormatAmount.ShouldBe(200);

        var accounts = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper,new GetAccountInfoDto
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        accounts.Count.ShouldBe(3);
        
        accounts.First(o => o.Address=="Address1").TokenHoldingCount.ShouldBe(2);
        accounts.First(o => o.Address=="Address2").TokenHoldingCount.ShouldBe(1);
        accounts.First(o => o.Address=="Address4").TokenHoldingCount.ShouldBe(1);
        
        var accountCollections = await Query.AccountCollection(AccountCollectionReadOnlyRepository, ObjectMapper,new GetAccountCollectionDto()
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        accountCollections.Items.Count.ShouldBe(2);
        accountCollections.Items[0].FormatAmount.ShouldBe(200);
        accountCollections.Items[1].FormatAmount.ShouldBe(300);
        
    }
}