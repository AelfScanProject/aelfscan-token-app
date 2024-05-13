using System.Linq.Expressions;
using AeFinder.Sdk;
using AElfScan.TokenApp.Entities;
using GraphQL;
using Volo.Abp.ObjectMapping;

namespace AElfScan.TokenApp.GraphQL;
public class Query
{
    public static async Task<TokenInfoPageResultDto> TokenInfo(
        [FromServices] IReadOnlyRepository<TokenInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTokenInfoDto input)
    {
        input.Validate();
        
        var queryable = await repository.GetQueryableAsync();
        
        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }
        
        if (!input.Symbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Symbol == input.Symbol);
        }

        if (!input.Symbols.IsNullOrEmpty())
        { 
            var predicates = input.Symbols.Select(s => 
                (Expression<Func<TokenInfo, bool>>)(o => o.Symbol == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }
        
        if (!input.TokenName.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.TokenName == input.TokenName);
        }
        
        if (!input.Owner.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Owner == input.Owner);
        }
        
        if (!input.Issuer.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Issuer == input.Issuer);
        }
        
        if (!input.CollectionSymbols.IsNullOrEmpty())
        {
            var predicates = input.CollectionSymbols.Select(s => 
                (Expression<Func<TokenInfo, bool>>)(o => o.CollectionSymbol == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }
        
        if (!input.Types.IsNullOrEmpty())
        {
            var predicates = input.Types.Select(s => 
                (Expression<Func<TokenInfo, bool>>)(o => o.Type == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);        
        }
        //add order by
        queryable = QueryableExtensions.TokenInfoSort(queryable, input);
        var totalCount = await QueryableExtensions.CountAsync(queryable);
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return new TokenInfoPageResultDto
        {
            TotalCount = totalCount,
            Items = objectMapper.Map<List<TokenInfo>, List<TokenInfoDto>>(result)
        };
    }
    
    public static async Task<List<AccountInfoDto>> AccountInfo(
        [FromServices] IReadOnlyRepository<AccountInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetAccountInfoDto input)
    {
        input.Validate();
        
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }
        
        if (!input.Address.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Address == input.Address);
        }
        
        
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return objectMapper.Map<List<AccountInfo>, List<AccountInfoDto>>(result);
    }
    
    public static async Task<AccountTokenPageResultDto> AccountToken(
        [FromServices] IReadOnlyRepository<AccountToken> repository,
        [FromServices] IObjectMapper objectMapper, GetAccountTokenDto input)
    {
        input.Validate();
        
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }
        
        if (!input.Address.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Address == input.Address);
        }
        
        if (!input.Symbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.Symbol == input.Symbol);
        }
        
        if (!input.CollectionSymbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.CollectionSymbol == input.CollectionSymbol);
        }
        
        if (!input.Types.IsNullOrEmpty())
        {
            var predicates = input.Types.Select(s => 
                (Expression<Func<AccountToken, bool>>)(o => o.Token.Type == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);        
        }
        
        queryable = QueryableExtensions.AccountTokenSort(queryable, input);

        var totalCount = await QueryableExtensions.CountAsync(queryable);
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return new AccountTokenPageResultDto
        {
            TotalCount = totalCount,
            Items = objectMapper.Map<List<AccountToken>, List<AccountTokenDto>>(result)
        };
    }
    
    public static async Task<TransferInfoPageResultDto> TransferInfo(
        [FromServices] IReadOnlyRepository<TransferInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTransferDto input)
    {
        input.Validate();
        
        var queryable = await repository.GetQueryableAsync();
        
        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }
        
        if (!input.Address.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.From == input.Address || o.To == input.Address);
        }
        
        if (!input.From.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.From == input.From);
        }
        
        if (!input.To.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.From == input.From);
        }
        
        if (!input.Symbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.Symbol == input.Symbol);
        }
        
        if (!input.CollectionSymbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.CollectionSymbol == input.CollectionSymbol);
        }
        
        if (!input.TransactionId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.TransactionId == input.TransactionId);
        } 
        
        if (!input.Search.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.TransactionId == input.Search || o.From == input.Search || o.To == input.Search
            || o.Token.Symbol == input.Search);
        } 
        
        if (!input.Types.IsNullOrEmpty())
        {
            var predicates = input.Types.Select(s => 
                (Expression<Func<TransferInfo, bool>>)(o => o.Token.Type == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);        
        }
        //add order by
        queryable = QueryableExtensions.TransferInfoSort(queryable, input);

        var totalCount = await QueryableExtensions.CountAsync(queryable);
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return new TransferInfoPageResultDto
        {
            TotalCount = totalCount,
            Items = objectMapper.Map<List<TransferInfo>, List<TransferInfoDto>>(result)
        };
    }
}