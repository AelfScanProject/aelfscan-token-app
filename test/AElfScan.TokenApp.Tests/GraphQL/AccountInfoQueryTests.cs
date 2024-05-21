using AElfScan.TokenApp.Processors;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.GraphQL;

public class AccountInfoQueryTests : TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;

    public AccountInfoQueryTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
    }
    
    [Fact]
    public async Task AccountInfo_WrongMaxResultCount_Test()
    {
        await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto()
        {
            ChainId = ChainId,
            MaxResultCount = 1001
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task AccountInfo_Test()
    {
        await CreateTokenAsync();
        
        var addresses = new List<string>
        {
            "xUgvBLughMpZp1w2E1GmgACU9h8EzqY5X4ZBqSKRRc4g9QL72",
            "zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8"
        };

        foreach (var address in addresses)
        {
            var transferred = new Transferred
            {
                Amount = 1,
                From = TestAddress,
                Symbol = "ELF",
                To = Address.FromBase58(address),
                Memo = "memo"
            };
            var logEventContext = GenerateLogEventContext(transferred);
            await _transferredProcessor.ProcessAsync(logEventContext);
        }
        await SaveDataAsync();

        var list = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto()
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Count.ShouldBe(3);
        
        list = await Query.AccountInfo(AccountInfoReadOnlyRepository, ObjectMapper, new GetAccountInfoDto()
        {
            Address = addresses[0],
            SkipCount = 0,
            MaxResultCount = 10
        });
        list.Count.ShouldBe(1);
    }
}