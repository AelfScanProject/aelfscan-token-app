using AeFinder.App.BlockProcessing;
using AeFinder.App.BlockState;
using AeFinder.App.OperationLimits;
using AeFinder.Block.Dtos;
using AeFinder.Grains.Grain.BlockStates;
using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.Orleans.TestBase;
using AElfScan.TokenApp.Processors;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;

namespace AElfScan.TokenApp;

public abstract class TokenContractAppTestBase: AElfScanTokenAppOrleansTestBase<AElfScanTokenAppTestModule>
{
    private readonly IAppDataIndexManagerProvider _appDataIndexManagerProvider;
    private readonly IAppBlockStateSetProvider _appBlockStateSetProvider;
    private readonly IOperationLimitManager _operationLimitManager;
    private readonly IBlockProcessingContext _blockProcessingContext;
    protected readonly TokenCreatedProcessor TokenCreatedProcessor;
    protected readonly IssuedProcessor IssuedProcessor;
    protected readonly IObjectMapper ObjectMapper;
    
    protected IEntityRepository<Entities.TokenInfo> TokenRepository;
    protected IEntityRepository<AccountToken> AccountTokenRepository;
    protected IEntityRepository<AccountInfo> AccountInfoRepository;
    protected IEntityRepository<TransferInfo> TransferInfoRepository;
    
    protected readonly IReadOnlyRepository<AccountInfo> AccountInfoReadOnlyRepository;
    protected readonly IReadOnlyRepository<AccountToken> AccountTokenReadOnlyRepository;
    protected readonly IReadOnlyRepository<Entities.TokenInfo> TokenInfoReadOnlyRepository;
    protected readonly IReadOnlyRepository<TransferInfo> TransferInfoReadOnlyRepository;
    
    protected Address TestAddress = Address.FromBase58("ooCSxQ7zPw1d4rhQPBqGKB6myvuWbicCiw3jdcoWEMMpa54ea");
    protected string ChainId = "AELF";
    protected string BlockHash = "dac5cd67a2783d0a3d843426c2d45f1178f4d052235a907a0d796ae4659103b1";
    protected string PreviousBlockHash = "e38c4fb1cf6af05878657cb3f7b5fc8a5fcfb2eec19cd76b73abb831973fbf4e";
    protected string TransactionId = "c1e625d135171c766999274a00a7003abed24cfe59a7215aabf1472ef20a2da2";
    protected long BlockHeight = 100;
    
    public TokenContractAppTestBase()
    {
        _appDataIndexManagerProvider = GetRequiredService<IAppDataIndexManagerProvider>();
        _appBlockStateSetProvider = GetRequiredService<IAppBlockStateSetProvider>();
        _operationLimitManager = GetRequiredService<IOperationLimitManager>();
        _blockProcessingContext = GetRequiredService<IBlockProcessingContext>();
        
        TokenCreatedProcessor = GetRequiredService<TokenCreatedProcessor>();
        IssuedProcessor = GetRequiredService<IssuedProcessor>();
        ObjectMapper = GetRequiredService<IObjectMapper>();
        TokenRepository = GetRequiredService<IEntityRepository<Entities.TokenInfo>>();
        AccountTokenRepository = GetRequiredService<IEntityRepository<AccountToken>>();
        AccountInfoRepository = GetRequiredService<IEntityRepository<AccountInfo>>();
        TransferInfoRepository = GetRequiredService<IEntityRepository<TransferInfo>>();
        AccountInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<AccountInfo>>();
        AccountTokenReadOnlyRepository = GetRequiredService<IReadOnlyRepository<AccountToken>>();
        TokenInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<Entities.TokenInfo>>();
        TransferInfoReadOnlyRepository = GetRequiredService<IReadOnlyRepository<TransferInfo>>();

        AsyncHelper.RunSync(async () => await InitializeBlockStateSetAsync());
    }

    protected async Task InitializeBlockStateSetAsync()
    {
        await _appBlockStateSetProvider.AddBlockStateSetAsync(ChainId, new BlockStateSet
        {
            Block = new BlockWithTransactionDto
            {
                ChainId = ChainId,
                BlockHash = BlockHash,
                PreviousBlockHash = PreviousBlockHash,
                BlockHeight = BlockHeight
            },
            Changes = new (),
            Processed = false
        });
        await _appBlockStateSetProvider.SetLongestChainBlockStateSetAsync(ChainId, BlockHash);
        
        _operationLimitManager.ResetAll();
        _blockProcessingContext.SetContext(ChainId, BlockHash, BlockHeight,
            DateTime.UtcNow, false);
    }

    protected LogEventContext GenerateLogEventContext<T>(T eventData) where T : IEvent<T>
    {
        var logEvent = eventData.ToLogEvent().ToSdkLogEvent();
        
        return new LogEventContext
        {
            ChainId = ChainId,
            Block = new LightBlock
            {
                BlockHash = BlockHash,
                BlockHeight = BlockHeight,
                BlockTime = DateTime.UtcNow,
                PreviousBlockHash = PreviousBlockHash
            },
            Transaction = new AeFinder.Sdk.Processor.Transaction()
            {
                TransactionId = TransactionId
            },
            LogEvent = logEvent
        };
    }

    protected async Task SaveDataAsync()
    {
        await _appDataIndexManagerProvider.SavaDataAsync();
        await _appBlockStateSetProvider.SetBestChainBlockStateSetAsync(ChainId, BlockHash);
    }

    protected async Task CreateTokenAsync()
    {
        var tokenCreated = new TokenCreated
        {
            Symbol = "ELF",
            Decimals = 8,
            IsBurnable = true,
            Issuer = TestAddress,
            IssueChainId = 9999721,
            TotalSupply = 1000,
            TokenName = "ELF Token",
            ExternalInfo = new ExternalInfo
            {
                Value = { {"key1","value1"} }
            }
        };
        var logEventContext = GenerateLogEventContext(tokenCreated);
        await TokenCreatedProcessor.ProcessAsync(logEventContext);
        
        var issued = new Issued
        {
            Amount = 100,
            Symbol = "ELF",
            To = TestAddress,
            Memo = "memo"
        };
        logEventContext = GenerateLogEventContext(issued);
        await IssuedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
    }
    
    protected async Task CreateCollectionTokenAsync()
    {
        
        var tokenCreated = new TokenCreated
        {
            Symbol = "NFT-0",
            Decimals = 0,
            IsBurnable = true,
            Issuer = TestAddress,
            IssueChainId = 9999721,
            TotalSupply = 1000,
            TokenName = "Collection Token"
        };
        var logEventContext = GenerateLogEventContext(tokenCreated);
        await TokenCreatedProcessor.ProcessAsync(logEventContext);
        
        var issued = new Issued
        {
            Amount = 100,
            Symbol = "NFT-0",
            To = TestAddress,
            Memo = "memo"
        };
        logEventContext = GenerateLogEventContext(issued);
        await IssuedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
    }
    
    protected async Task CreateNftTokenAsync()
    {
        var tokenCreated = new TokenCreated
        {
            Symbol = "NFT-1",
            Decimals = 0,
            IsBurnable = true,
            Issuer = TestAddress,
            IssueChainId = 9999721,
            TotalSupply = 1000,
            TokenName = "NFT Token"
        };
        var logEventContext = GenerateLogEventContext(tokenCreated);
        await TokenCreatedProcessor.ProcessAsync(logEventContext);
        
        var issued = new Issued
        {
            Amount = 100,
            Symbol = "NFT-1",
            To = TestAddress,
            Memo = "memo"
        };
        logEventContext = GenerateLogEventContext(issued);
        await IssuedProcessor.ProcessAsync(logEventContext);
        await SaveDataAsync();
    }
}