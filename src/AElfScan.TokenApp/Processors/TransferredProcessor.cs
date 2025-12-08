using AeFinder.Sdk;
using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;
using AutoMapper.Internal.Mappers;

namespace AElfScan.TokenApp.Processors;

public class TransferredProcessor : TokenProcessorBase<Transferred>
{
    private readonly IBlockChainService _blockChainService;
    public TransferredProcessor(IBlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }
    public override async Task ProcessAsync(Transferred logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        if (token == null)
        {
            Logger.LogError($"start TransferredProcessor ProcessAsync symbol:{logEvent.Symbol} token is null" );
            var tokenInfo =  await _blockChainService.ViewContractAsync<AElf.Contracts.MultiToken.TokenInfo>(
                context.ChainId, GetContractAddress(context.ChainId),
                "GetTokenInfo", new GetTokenInfoInput
                {
                    Symbol = logEvent.Symbol
                });
            if (tokenInfo.Decimals != 0 || tokenInfo.TotalSupply != 1)
            {
                return;
            }

            token = new Entities.TokenInfo
            {
                Id = IdGenerateHelper.GetId(context.ChainId,
                    logEvent.Symbol),
                Type = TokenSymbolHelper.GetSymbolType(logEvent.Symbol),
                CollectionSymbol = TokenSymbolHelper.GetCollectionSymbol(logEvent.Symbol),
                Symbol = tokenInfo.Symbol,
                LowerCaseSymbol = tokenInfo.Symbol.ToLower(),
                Decimals = tokenInfo.Decimals,
                TokenName = tokenInfo.TokenName,
                TotalSupply = tokenInfo.TotalSupply,
                Supply = logEvent.Amount,
                Issued = tokenInfo.Issued,
                Issuer = tokenInfo.Issuer?.ToBase58(),
                Owner = tokenInfo.Owner?.ToBase58(),
                IsBurnable = tokenInfo.IsBurnable,
                IssueChainId = tokenInfo.IssueChainId.ToString(),
                ExternalInfo = tokenInfo.ExternalInfo?.Value?.ToDictionary(o => o.Key, o => o.Value)
            };
          
            if (token.Owner.IsNullOrWhiteSpace())
            {
                token.Owner = token.Issuer;
            }

            if (!token.TokenName.IsNullOrWhiteSpace())
            {
                token.LowerCaseTokenName = token.TokenName.ToLower();
            }
        }
        await SaveEntityAsync(token);
        

        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "Transfer";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        await AddTransferAsync(transfer, context);
        
        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.From.ToBase58(), -logEvent.Amount);
        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.To.ToBase58(), logEvent.Amount);
        
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.From.ToBase58(), logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.To.ToBase58(), logEvent.Symbol);
    }
}