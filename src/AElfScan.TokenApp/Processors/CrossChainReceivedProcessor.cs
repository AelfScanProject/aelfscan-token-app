using AeFinder.Sdk;
using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;
using Microsoft.Extensions.Logging;

namespace AElfScan.TokenApp.Processors;

public class CrossChainReceivedProcessor : TokenProcessorBase<CrossChainReceived>
{
    private readonly IBlockChainService _blockChainService;
    public CrossChainReceivedProcessor(IBlockChainService blockChainService)
    {
        _blockChainService = blockChainService;
    }
    public override async Task ProcessAsync(CrossChainReceived logEvent, LogEventContext context)
    {      
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        if (token == null)
        {
          var tokenInfo =  await _blockChainService.ViewContractAsync<AElf.Contracts.MultiToken.TokenInfo>(
                context.ChainId, GetContractAddress(context.ChainId),
                "GetTokenInfo", new GetTokenInfoInput
                {
                    Symbol = logEvent.Symbol
                });
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
              Supply = 0,
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
        token.Supply += logEvent.Amount;
        await SaveEntityAsync(token);
        await ChangeCollectionItemCountAsync(context, token, logEvent.Amount);

        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "CrossChainReceive";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        transfer.ToChainId = context.ChainId;
        await AddTransferAsync(transfer, context);

        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.To.ToBase58(), logEvent.Amount);
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.To.ToBase58(), logEvent.Symbol);
    }
}