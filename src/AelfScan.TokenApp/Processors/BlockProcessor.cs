using AeFinder.Sdk;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;
using Volo.Abp.ObjectMapping;
using TokenInfo = AElfScan.TokenApp.Entities.TokenInfo;

namespace AElfScan.TokenApp.Processors;

public class BlockProcessor : BlockProcessorBase
{
    private readonly IInitialBalanceProvider _initialBalanceProvider;
    private readonly IBlockChainService _blockChainService;
    private readonly ITokenContractAddressProvider _tokenContractAddressProvider;
    private readonly IObjectMapper _objectMapper;

    public BlockProcessor(IInitialBalanceProvider initialBalanceProvider, IBlockChainService blockChainService,
        ITokenContractAddressProvider tokenContractAddressProvider, IObjectMapper objectMapper)
    {
        _initialBalanceProvider = initialBalanceProvider;
        _blockChainService = blockChainService;
        _tokenContractAddressProvider = tokenContractAddressProvider;
        _objectMapper = objectMapper;
    }

    public override async Task ProcessAsync(Block block, BlockContext context)
    {
        await ProcessInitialBalanceAsync(block, context);
    }

    private async Task ProcessInitialBalanceAsync(Block block, BlockContext context)
    {
        if (!TokenAppConstants.InitialBalanceEndHeight.TryGetValue(context.ChainId, out var height) ||
            block.BlockHeight > height)
        {
            return;
        }

        var balances = _initialBalanceProvider.GetInitialBalances(context.ChainId, block.BlockHeight);

        foreach (var balance in balances)
        {
            var initialBalance = balance.Split(',');
            var address = initialBalance[0];
            var symbol = initialBalance[1];
            var amount = long.Parse(initialBalance[2]);
            
            var token = await GetTokenAsync(context, symbol);
            var id = IdGenerateHelper.GetId(context.ChainId, address, symbol);
            var accountToken = await GetEntityAsync<AccountToken>(id);
            if (accountToken == null)
            {
                accountToken = new AccountToken
                {
                    Id = id,
                    Token = _objectMapper.Map<Entities.TokenInfo,TokenBase>(token),
                    Address = address,
                };
            }

            var firstInitAccountToken = accountToken.Amount == 0;

            accountToken.Amount = amount;
            accountToken.FormatAmount = accountToken.Amount / (decimal)Math.Pow(10, accountToken.Token.Decimals);
            
            await SaveEntityAsync(accountToken);

            if (!firstInitAccountToken) 
                continue;
            
            await IncreaseHolderCountAsync(context, token);
            var accountId = IdGenerateHelper.GetId(context.ChainId, address);
            var accountInfo = await GetEntityAsync<AccountInfo>(accountId);
            if (accountInfo == null)
            {
                accountInfo = new AccountInfo
                {
                    Id = IdGenerateHelper.GetId(context.ChainId, address),
                    Address = address
                };
            }

            accountInfo.TokenHoldingCount += 1;
            await SaveEntityAsync(accountInfo);
        }
    }
    
    private async Task<Entities.TokenInfo> GetTokenAsync(BlockContext context, string symbol)
    {
        var tokenId = IdGenerateHelper.GetId(context.ChainId, symbol);
        var token = await GetEntityAsync<TokenInfo>(tokenId);
        if (token == null)
        {
            var tokenFromChain = await _blockChainService.ViewContractAsync<AElf.Contracts.MultiToken.TokenInfo>(context.ChainId,
                _tokenContractAddressProvider.GetContractAddress(context.ChainId), "GetTokenInfo",
                new GetTokenInfoInput
                {
                    Symbol = symbol
                });
            token = new Entities.TokenInfo
            {
                Id = tokenId,
                Supply = tokenFromChain.Supply,
                TotalSupply = tokenFromChain.TotalSupply,
                Symbol = symbol,
                Decimals = tokenFromChain.Decimals,
                TokenName = tokenFromChain.TokenName,
                Type = TokenSymbolHelper.GetSymbolType(symbol),
                CollectionSymbol = TokenSymbolHelper.GetCollectionSymbol(symbol),
                ExternalInfo = tokenFromChain.ExternalInfo?.Value?.ToDictionary(o => o.Key, o => o.Value)
            };
            await SaveEntityAsync(token);
        }

        return token;
    }

    protected async Task IncreaseHolderCountAsync(BlockContext context, Entities.TokenInfo tokenInfo, bool ignoreCollection = true)
    {
        if (ignoreCollection && tokenInfo.Type == SymbolType.NftCollection)
        {
            return;
        }

        tokenInfo.HolderCount += 1;
        await SaveEntityAsync(tokenInfo);
        
        if (tokenInfo.Type == SymbolType.Nft)
        {
            var tokenCollection = await GetTokenAsync(context, tokenInfo.CollectionSymbol);
            await IncreaseHolderCountAsync(context, tokenCollection, false);
        }
    }
}