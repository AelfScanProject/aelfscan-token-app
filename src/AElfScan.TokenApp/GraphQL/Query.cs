using System.Linq.Expressions;
using AeFinder.Sdk;
using AElfScan.TokenApp.Entities;
using GraphQL;
using Volo.Abp.ObjectMapping;

namespace AElfScan.TokenApp.GraphQL;

public class Query
{
    private static readonly List<string> InitSymbolList = new()
    {
        "ELF", "SHARE", "VOTE", "CPU", "WRITE", "READ", "NET", "RAM", "STORAGE"
    };


    public static async Task<TokenInfoPageResultDto> TokenInfo(
        [FromServices] IReadOnlyRepository<TokenInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTokenInfoDto input)
    {
        input.Validate();

        var queryable = await repository.GetQueryableAsync();

        if (input.BeginBlockTime != null)
        {
            queryable = queryable.Where(o => o.Metadata.Block.BlockTime > input.BeginBlockTime);
            queryable = queryable.Where(o => o.TransferCount == 0);
        }

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

            if (input.Types.Contains(SymbolType.Token))
            {
                predicates = predicates.Concat(new Expression<Func<TokenInfo, bool>>[]
                {
                    o => o.Type == SymbolType.Token
                });

                // Add A new condition to check whether o.Symbol is equal to the element A or B in the list
                var symbolPredicates = TokenAppConstants.SpecialSymbolList.Select(s =>
                    (Expression<Func<TokenInfo, bool>>)(o => o.Symbol == s));

                predicates = predicates.Concat(symbolPredicates);
            }

            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }


        if (!input.Search.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Symbol.Contains(input.Search) || o.TokenName.Contains(input.Search));
        }

        if (!input.ExactSearch.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Symbol == input.ExactSearch || o.TokenName == input.ExactSearch);
        }

        var totalCount = 0;
        if (!input.FuzzySearch.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.LowerCaseSymbol.Contains(input.FuzzySearch)
                                             || o.LowerCaseTokenName.Contains(input.FuzzySearch));
        }
        else
        {
            totalCount = await QueryableExtensions.CountAsync(queryable);
        }

        //add order by
        queryable = QueryableExtensions.TokenInfoSort(queryable, input);

        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        // not needed after resubscribing
        if (!result.IsNullOrEmpty())
        {
            foreach (var token in result)
            {
                if (token.Metadata.ChainId != "AELF" && InitSymbolList.Contains(token.Symbol))
                {
                    token.Supply -= token.TotalSupply;
                }
            }
        }

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


    public static async Task<List<DailyHolderDto>> DailyHolder(
        [FromServices] IReadOnlyRepository<DailyHolderInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetDailyHolderDto input)
    {
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.ChainId == input.ChainId);
        }


        var result = queryable.Take(10000).ToList();
        return objectMapper.Map<List<DailyHolderInfo>, List<DailyHolderDto>>(result);
    }


    public static async Task<AccountCountDto> AccountCount(
        [FromServices] IReadOnlyRepository<AccountInfo> repository, GetAccountCountDto input)
    {
        var queryable = await repository.GetQueryableAsync();

        if (!input.ChainId.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        }


        var count = queryable.Count();

        return new AccountCountDto() { Count = count };
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

            if (input.Types.Contains(SymbolType.Token))
            {
                predicates = predicates.Concat(new Expression<Func<AccountToken, bool>>[]
                {
                    o => o.Token.Type == SymbolType.Token
                });

                // Add A new condition to check whether o.Symbol is equal to the element A or B in the list
                var symbolPredicates = TokenAppConstants.SpecialSymbolList.Select(s =>
                    (Expression<Func<AccountToken, bool>>)(o => o.Token.Symbol == s));

                predicates = predicates.Concat(symbolPredicates);
            }

            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.Symbols.IsNullOrEmpty())
        {
            var predicates = input.Symbols.Select(s =>
                (Expression<Func<AccountToken, bool>>)(o => o.Token.Symbol == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.AddressList.IsNullOrEmpty())
        {
            var predicates = input.AddressList.Select(s =>
                (Expression<Func<Entities.AccountToken, bool>>)(o => o.Address == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.SearchSymbols.IsNullOrEmpty())
        {
            var predicates = input.SearchSymbols.Select(s =>
                (Expression<Func<AccountToken, bool>>)(o => o.Token.Symbol == s || o.Token.CollectionSymbol == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.Search.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.Symbol == input.Search || o.Address == input.Search);
        }

        if (!input.FuzzySearch.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.LowerCaseSymbol.Contains(input.FuzzySearch)
                                             || o.LowerCaseAddress.Contains(input.FuzzySearch));
        }

        if (!input.OrderInfos.IsNullOrEmpty())
        {
            var orderInfos = input.OrderInfos.Where(c => c.OrderBy == "FirstNftTime").ToList();
            if (!orderInfos.IsNullOrEmpty())
            {
                var orderInfo = orderInfos.First();

                queryable = orderInfo.Sort == SortType.Desc.ToString()
                    ? queryable.OrderByDescending(c => c.FirstNftTime)
                    : queryable.OrderBy(c => c.FirstNftTime);
            }
            else
            {
                queryable = QueryableExtensions.AccountTokenSort(queryable, input);
            }
        }

        if (input.AmountGreaterThanZero != null && input.AmountGreaterThanZero.Value)
        {
            queryable = queryable.Where(o => o.Amount > 0);
        }


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
        if (input.BeginBlockTime != null)
        {
            queryable = queryable.Where(o => o.Metadata.Block.BlockTime > input.BeginBlockTime);
        }

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
            queryable = queryable.Where(o => o.TransactionId == input.Search || o.From == input.Search ||
                                             o.To == input.Search
                                             || o.Token.Symbol == input.Search);
        }

        if (!input.FuzzySearch.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.TransactionId.Contains(input.FuzzySearch) ||
                                             o.LowerCaseFrom.Contains(input.FuzzySearch) ||
                                             o.LowerCaseTo.Contains(input.FuzzySearch)
                                             || o.Token.LowerCaseSymbol.Contains(input.FuzzySearch));
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


    public static async Task<TransferInfoByBlockPageResultDto> TransferInfoByBlock(
        [FromServices] IReadOnlyRepository<TransferInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetTransferByBlockDto input)
    {
        input.Validate();
        var queryable = await repository.GetQueryableAsync();
        queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        queryable = queryable.Where(o => o.Metadata.Block.BlockHeight >= input.BeginBlockHeight);
        if (input.EndBlockHeight is > 0)
        {
            queryable = queryable.Where(o => o.Metadata.Block.BlockHeight <= input.EndBlockHeight);
        }

        if (!input.SymbolList.IsNullOrEmpty())
        {
            var predicates = input.SymbolList.Select(s =>
                (Expression<Func<TransferInfo, bool>>)(o => o.Token.Symbol == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.FromList.IsNullOrEmpty())
        {
            var predicates = input.FromList.Select(s =>
                (Expression<Func<Entities.TransferInfo, bool>>)(o => o.From == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.ToList.IsNullOrEmpty())
        {
            var predicates = input.ToList.Select(s =>
                (Expression<Func<Entities.TransferInfo, bool>>)(o => o.To == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.Methods.IsNullOrEmpty())
        {
            var predicates = input.Methods.Select(s =>
                (Expression<Func<Entities.TransferInfo, bool>>)(o => o.Method == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        //add order by
        queryable = QueryableExtensions.TransferInfoSort(queryable, input);

        var totalCount = await QueryableExtensions.CountAsync(queryable);
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return new TransferInfoByBlockPageResultDto()
        {
            TotalCount = totalCount,
            Items = objectMapper.Map<List<TransferInfo>, List<TransferInfoDto>>(result)
        };
    }

    public static async Task<BlockBurnFeeListDto> BlockBurnFeeInfo(
        [FromServices] IReadOnlyRepository<BlockBurnFeeInfo> repository,
        [FromServices] IObjectMapper objectMapper, GetBlockBurnFeeDto input)
    {
        var queryable = await repository.GetQueryableAsync();
        var rangeLimit = 100;
        if (input.EndBlockHeight > input.BeginBlockHeight + rangeLimit)
        {
            throw new ArgumentOutOfRangeException(
                $"Max block range limit for block height is {rangeLimit}.");
        }

        queryable = queryable.Where(o => o.Metadata.ChainId == input.ChainId);
        queryable = queryable.Where(o => o.BlockHeight >= input.BeginBlockHeight);
        queryable = queryable.Where(o => o.BlockHeight <= input.EndBlockHeight);
        queryable = queryable.Where(o => o.Symbol == TokenAppConstants.BaseTokenSymbol);
        var result = queryable.ToList();
        return new BlockBurnFeeListDto
        {
            Items = objectMapper.Map<List<BlockBurnFeeInfo>, List<BlockBurnFeeDto>>(result)
        };
    }


    public static async Task<AccountCollectionPageResultDto> AccountCollection(
        [FromServices] IReadOnlyRepository<AccountCollection> repository,
        [FromServices] IObjectMapper objectMapper, GetAccountCollectionDto input)
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

        if (!input.AddressList.IsNullOrEmpty())
        {
            var predicates = input.AddressList.Select(s =>
                (Expression<Func<Entities.AccountCollection, bool>>)(o => o.Address == s));
            var predicate = predicates.Aggregate((prev, next) => prev.Or(next));
            queryable = queryable.Where(predicate);
        }

        if (!input.Symbol.IsNullOrWhiteSpace())
        {
            queryable = queryable.Where(o => o.Token.Symbol == input.Symbol);
        }

        if (input.AmountGreaterThanZero != null && input.AmountGreaterThanZero.Value)
        {
            queryable = queryable.Where(o => o.FormatAmount > 0);
        }
        queryable = QueryableExtensions.AccountCollectionSort(queryable, input);

        var totalCount = await QueryableExtensions.CountAsync(queryable);
        var result = queryable.Skip(input.SkipCount)
            .Take(input.MaxResultCount).ToList();
        return new AccountCollectionPageResultDto()
        {
            TotalCount = totalCount,
            Items = objectMapper.Map<List<AccountCollection>, List<AccountCollectionDto>>(result)
        };
    }
}