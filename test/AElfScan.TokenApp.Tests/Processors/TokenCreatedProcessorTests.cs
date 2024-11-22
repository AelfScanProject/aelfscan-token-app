using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.GraphQL;
using AElf;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TokenCreatedProcessorTests : TokenContractAppTestBase
{
    private readonly TokenCreatedProcessor _tokenCreatedProcessor;

    public TokenCreatedProcessorTests()
    {
        _tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
    }

    [Theory]
    [InlineData("ELF", SymbolType.Token)]
    [InlineData("NFT-1", SymbolType.Nft)]
    [InlineData("NFT-0", SymbolType.NftCollection)]
    public async Task HandleEventAsync_Test(string symbol, SymbolType type)
    {
        var tokenCreated = new TokenCreated
        {
            Symbol = symbol,
            Decimals = 8,
            IsBurnable = true,
            Issuer = Address.FromBase58("ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea"),
            IssueChainId = 9999721,
            TotalSupply = 1000,
            TokenName = "ELF Token",
            ExternalInfo = new ExternalInfo
            {
                Value = { { "key1", "value1" } }
            }
        };

        var logEventContext = GenerateLogEventContext(tokenCreated);

        await _tokenCreatedProcessor.ProcessAsync(logEventContext);

        var token = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto
        {
            ChainId = ChainId,
            Symbols = new List<string>() {tokenCreated.Symbol}
        });
        token.Items[0].Symbol.ShouldBe(tokenCreated.Symbol);
        token.Items[0].Decimals.ShouldBe(tokenCreated.Decimals);
        token.Items[0].IsBurnable.ShouldBe(tokenCreated.IsBurnable);
        token.Items[0].Issuer.ShouldBe(tokenCreated.Issuer.ToBase58());
        token.Items[0].IssueChainId.ShouldBe(ChainHelper.ConvertChainIdToBase58(tokenCreated.IssueChainId));
        token.Items[0].TotalSupply.ShouldBe(tokenCreated.TotalSupply);
        token.Items[0].TokenName.ShouldBe(tokenCreated.TokenName);
        token.Items[0].Type.ShouldBe(type);
        token.Items[0].Supply.ShouldBe(0);
        token.Items[0].Issued.ShouldBe(0);
        token.Items[0].HolderCount.ShouldBe(0);
        token.Items[0].TransferCount.ShouldBe(0);
        token.Items[0].ExternalInfo.Count.ShouldBe(1);
        token.Items[0].ExternalInfo[0].Key.ShouldBe("key1");
        token.Items[0].ExternalInfo[0].Value.ShouldBe("value1");
        token.Items[0].IsPrimaryToken.ShouldBe(false);
        if (type == SymbolType.Nft)
            token.Items[0].CollectionSymbol.ShouldBe("NFT-0");
        else
            token.Items[0].CollectionSymbol.ShouldBeNull();
    }
}