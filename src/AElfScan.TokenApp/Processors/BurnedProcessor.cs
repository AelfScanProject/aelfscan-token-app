using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class BurnedProcessor : TokenProcessorBase<Burned>
{
    public override async Task ProcessAsync(Burned logEvent, LogEventContext context)
    {
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        token.Supply -= logEvent.Amount;
        await SaveEntityAsync(token);

        var transfer = new TransferInfo();
        ObjectMapper.Map(logEvent, transfer);
        transfer.Method = "Burn";
        transfer.Token = ObjectMapper.Map<Entities.TokenInfo, TokenBase>(token);
        transfer.From = logEvent.Burner.ToBase58();
        await AddTransferAsync(transfer, context);

        await ModifyBalanceAsync(context, logEvent.Symbol, logEvent.Burner.ToBase58(), -logEvent.Amount);
        await IncreaseTokenInfoTransferCountAsync(context, logEvent.Symbol);
        await IncreaseAccountTransferCountAsync(context, logEvent.Burner.ToBase58(), logEvent.Symbol);
        await SaveBurnFeeAsync(context, logEvent);
    }

    private async Task SaveBurnFeeAsync(LogEventContext context,Burned burned)
    {
        if(TokenSymbolHelper.GetSymbolType(burned.Symbol) ==  SymbolType.Nft && 
           "SEED-0".Equals(TokenSymbolHelper.GetCollectionSymbol(burned.Symbol)))
        {
            return;
        }
        if (TokenAppConstants.AddressListMap.TryGetValue(context.ChainId, out var addressList) && addressList.Contains(burned.Burner.ToBase58()))
        {
            var burnFeeInfoId=  IdGenerateHelper.GetId(context.ChainId, context.Block.BlockHeight,burned.Symbol);
            var burnFeeInfo = await GetEntityAsync<BlockBurnFeeInfo>(burnFeeInfoId);
            if (burnFeeInfo == null)
            {
                burnFeeInfo = new BlockBurnFeeInfo();
                burnFeeInfo.Id = burnFeeInfoId;
                burnFeeInfo.Symbol = burned.Symbol;
            }
            burnFeeInfo.Amount += burned.Amount;
            burnFeeInfo.BlockHeight = context.Block.BlockHeight;
            await SaveEntityAsync(burnFeeInfo);
        }
    }
}