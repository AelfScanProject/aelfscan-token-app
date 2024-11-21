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
            MaxResultCount = 100,
            OrderBy = "BlockHeight",
            Sort = "Asc",
            SearchAfter = new List<string> { "200000001"  }
        });
        list.Items.Count.ShouldBe(0);
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            Address = TestAddress.ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100,
            OrderInfos = new List<OrderInfo>()
            {
                new OrderInfo()
                {
                   OrderBy = "BlockHeight",
                   Sort = "Asc"
                }
            },
            SearchAfter = new List<string> { "200000001"  }
        });
        list.Items.Count.ShouldBe(0);
        
        list = await Query.TransferInfo(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferDto()
        {
            Address = TestAddress.ToBase58(),
            SkipCount = 0,
            MaxResultCount = 100,
            OrderInfos = new List<OrderInfo>()
            {
                new OrderInfo()
                {
                    OrderBy = "BlockHeight",
                    Sort = "Desc"
                },
                new OrderInfo()
                {
                    OrderBy = "FormatAmount",
                    Sort = "Desc"
                }
            },
            SearchAfter = new List<string> { "100", "1" }
        });
        list.Items.Count.ShouldBe(3);
        
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
        
       var  listByBlock = await Query.TransferInfoByBlock(TransferInfoReadOnlyRepository, ObjectMapper, new GetTransferByBlockDto()
        {
            ChainId = "AELF",
            SkipCount = 0,
            MaxResultCount = 100,
            BeginBlockHeight = 98,
            EndBlockHeight = 101,
            SymbolList = new List<string>()
            {
                "ELF"
            },
            FromList = new List<string>()
            {
                TestAddress.ToBase58()
            },
            ToList = new List<string>()
            {
                "zBVzvebV9CvyFAcmzZ7uj9MZLMHf2t1xfkECEEpvcUyTa3XU8"
            },
            Methods = new List<string>()
            {
                "Transfer"
            }
            
        });
        listByBlock.Items.Count.ShouldBe(1);
    }
    
    
   
}