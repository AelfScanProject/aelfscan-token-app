using AeFinder.Sdk;
using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.Processors;
using AElf.Contracts.MultiToken;
using AElf.Types;
using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.GraphQL;

public class TransferInfoQueryTests : TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;

    public TransferInfoQueryTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
    }
    
    [Fact]
    public async Task TransferInfo_WrongMaxResultCount_Test()
    {
        await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            ChainId = ChainId,
            MaxResultCount = 1001
        }).ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task AccountToken_Test()
    {
        await CreateTokenAsync();
        await CreateCollectionTokenAsync();

        var addresses = new List<string>
        {
            "xUgvBLughMpZp1w2E1GmgACU9h8EzqY5X4ZBqSKRRc4g9QL72",
            "zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8"
        };

        foreach (var address in addresses)
        {
            var transferred = new Transferred
            {
                Amount = 10,
                From = TestAddress,
                Symbol = "ELF",
                To = Address.FromBase58(address),
                Memo = "memo"
            };
            var logEventContext = GenerateLogEventContext(transferred);            
            await _transferredProcessor.ProcessAsync(logEventContext);
            await SaveDataAsync();
        }
        {
            var transferred = new Transferred
            {
                Amount = 1,
                From = Address.FromBase58(addresses[0]),
                Symbol = "ELF",
                To = Address.FromBase58(addresses[1]),
                Memo = "memo"
            };
            var logEventContext = GenerateLogEventContext(transferred);
            await _transferredProcessor.ProcessAsync(logEventContext);
            await SaveDataAsync();
        }

        var list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            ChainId = ChainId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(5);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            Address = TestAddress.ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(4);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            Address = addresses[1],
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(2);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            From = TestAddress.ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(2);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            To = TestAddress.ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(2);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            Symbol = "ELF",
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(4);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            TransactionId = TransactionId,
            SkipCount = 0,
            MaxResultCount = 100
        });
        list.Items.Count.ShouldBe(5);
        
        // list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        // {
        //     Methods = new List<string>{"Transfer"},
        //     SkipCount = 0,
        //     MaxResultCount = 100
        // });
        // list.Count.ShouldBe(3);
    }
}