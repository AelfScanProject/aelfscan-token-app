
using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp.GraphQL;

public class QueryableExtensions
{
    public static Task<int> CountAsync<T>(IQueryable<T> query)
    {
        return Task.Run(query.Count);
    }
    
    public static IQueryable<TokenInfo> TokenInfoSort(IQueryable<TokenInfo> queryable, GetTokenInfoDto input)
    {
        var sortedQueryable = queryable;

        switch (input.OrderBy)
        {
            case "BlockTime":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.Metadata.Block.BlockTime) :
                    queryable.OrderByDescending(o => o.Metadata.Block.BlockTime);
                break;
            case "HolderCount":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.HolderCount) :
                    queryable.OrderByDescending(o => o.HolderCount);
                break;
            case "TransferCount":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.TransferCount) :
                    queryable.OrderByDescending(o => o.TransferCount);
                break;
            default:
                break;
        }
        return sortedQueryable;
    }
    
    public static IQueryable<TransferInfo> TransferInfoSort(IQueryable<TransferInfo> queryable, GetTransferDto input)
    {
        var sortedQueryable = queryable;

        switch (input.OrderBy)
        {
            case "BlockTime":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.Metadata.Block.BlockTime) :
                    queryable.OrderByDescending(o => o.Metadata.Block.BlockTime);
                break;
            case "FormatAmount":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.FormatAmount) :
                    queryable.OrderByDescending(o => o.FormatAmount);
                break;
            default:
                break;
        }
        return sortedQueryable;
    }
    
    public static IQueryable<AccountToken> AccountTokenSort(IQueryable<AccountToken> queryable, GetAccountTokenDto input)
    {
        var sortedQueryable = queryable;

        switch (input.OrderBy)
        {
            case "Symbol":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.Token.Symbol) :
                    queryable.OrderByDescending(o => o.Token.Symbol);
                break;
            case "BlockTime":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.Metadata.Block.BlockTime) :
                    queryable.OrderByDescending(o => o.Metadata.Block.BlockTime);
                break;
            case "FormatAmount":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.FormatAmount) :
                    queryable.OrderByDescending(o => o.FormatAmount);
                break;
            case "TransferCount":
                sortedQueryable = input.Sort == SortType.Asc.ToString() ?
                    queryable.OrderBy(o => o.TransferCount) :
                    queryable.OrderByDescending(o => o.TransferCount);
                break;
            default:
                break;
        }
        return sortedQueryable;
    }
}