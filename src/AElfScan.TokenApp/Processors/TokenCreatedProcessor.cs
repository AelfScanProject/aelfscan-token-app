using AeFinder.Sdk.Processor;
using AElf.Contracts.MultiToken;

namespace AElfScan.TokenApp.Processors;

public class TokenCreatedProcessor : TokenProcessorBase<TokenCreated>
{
    public override async Task ProcessAsync(TokenCreated logEvent, LogEventContext context)
    {
        var id = IdGenerateHelper.GetId(context.ChainId, logEvent.Symbol);
        var token = await GetTokenAsync(context.ChainId, logEvent.Symbol);
        if (token == null)
        {
            token = new Entities.TokenInfo
            {
                Id = id,
                Type = TokenSymbolHelper.GetSymbolType(logEvent.Symbol),
                CollectionSymbol = TokenSymbolHelper.GetCollectionSymbol(logEvent.Symbol)
            };
        }
        
        ObjectMapper.Map(logEvent, token);
        
        if (token.Owner.IsNullOrWhiteSpace())
        {
            token.Owner = token.Issuer;
        }

        if (!token.TokenName.IsNullOrWhiteSpace())
        {
            token.LowerCaseTokenName = token.TokenName.ToLower();
        }

        await SaveEntityAsync(token);
    }
}