using AeFinder.Sdk;
using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.Processors;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.GraphQL;

public class TokenInfoQueryTests : TokenContractAppTestBase
{
    private readonly TokenCreatedProcessor _tokenCreatedProcessor;

    public TokenInfoQueryTests()
    {
        _tokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
    }

    [Fact]
    public async Task TokenInfo_WrongMaxResultCount_Test()
    {
        await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            MaxResultCount = 1001
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task TokenInfo_Test()
    {
        await CreateCollectionTokenAsync();
        await CreateNftTokenAsync();

        for (var i = 0; i < 6; i++)
        {
            var tokenCreated = new TokenCreated
            {
                Symbol = "SGR-" + i,
                Decimals = 8,
                IsBurnable = true,
                Issuer = Address.FromBase58("xUgvBLughMpZp1w2E1GmgACU9h8EzqY5X4ZBqSKRRc4g9QL72"),
                Owner = Address.FromBase58("zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8"),
                IssueChainId = 9999721,
                TotalSupply = 1000,
                TokenName = "TokenName" + i,
                ExternalInfo = new ExternalInfo
                {
                    Value = { { "key1", "value1" } }
                }
            };

            var logEventContext = GenerateLogEventContext(tokenCreated);

            await _tokenCreatedProcessor.ProcessAsync(logEventContext);
        }

        await SaveDataAsync();

        var list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(8);

        list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            Symbol = "SGR-0",
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(1);

        list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            TokenName = "TokenName0",
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(1);

        list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            Issuer = "xUgvBLughMpZp1w2E1GmgACU9h8EzqY5X4ZBqSKRRc4g9QL72",
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(6);

        list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            Owner = "zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8",
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(6);

        list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        {
            ChainId = ChainId,
            Types = new List<SymbolType> { SymbolType.Nft, SymbolType.NftCollection },
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Items.Count.ShouldBe(8);

        // list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        // {
        //     ChainId = ChainId,
        //     PartialSymbol = "BOL0",
        //     SkipCount = 0,
        //     MaxResultCount = 10
        // });
        // list.Count.ShouldBe(1);
        //
        // list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        // {
        //     ChainId = ChainId,
        //     PartialTokenName = "NAME0",
        //     SkipCount = 0,
        //     MaxResultCount = 10
        // });
        // list.Count.ShouldBe(1);
        //
        // list = await Query.TokenInfo(TokenInfoReadOnlyRepository, ObjectMapper, new GetTokenInfoDto()
        // {
        //     ChainId = ChainId,
        //     PartialTokenName = "Name0",
        //     SkipCount = 0,
        //     MaxResultCount = 10
        // });
        // list.Count.ShouldBe(1);
    }
}