using System.Linq.Expressions;
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
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }

    public static IQueryable<TransferInfo> TransferInfoSort(IQueryable<TransferInfo> queryable, GetTransferDto input)
    {
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }
    
    public static IQueryable<TransferInfo> TransferInfoSort(IQueryable<TransferInfo> queryable, GetTransferByBlockDto input)
    {
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }

    public static IQueryable<AccountToken> AccountTokenSort(IQueryable<AccountToken> queryable,
        GetAccountTokenDto input)
    {
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }
    
    public static IQueryable<AccountCollection> AccountCollectionSort(IQueryable<AccountCollection> queryable,
        GetAccountCollectionDto input)
    {
        return ApplySortingAndPaging(queryable, input.GetAdaptableOrderInfos(), input.SearchAfter);
    }

    private static IQueryable<T> ApplySortingAndPaging<T>(IQueryable<T> queryable, List<OrderInfo> orderInfos,
        List<string> searchAfter)
    {
        if (!orderInfos.IsNullOrEmpty())
        {
            foreach (var orderInfo in orderInfos)
            {
                queryable = AddSort(queryable, orderInfo.OrderBy, orderInfo.Sort);
            }
        }

        if (searchAfter != null && searchAfter.Any())
        {
            queryable = queryable.After(searchAfter.Cast<object>().ToArray());
        }

        return queryable;
    }

    private static IQueryable<T> AddSort<T>(IQueryable<T> queryable, string orderBy, string sort)
    {
        var parameter = Expression.Parameter(typeof(T), "o");
        Expression property = null;
        switch (orderBy)
        {
            case "Id":
                property = GetNestedPropertyExpression(parameter, "Id");
                break;
            case "BlockTime":
                property = GetNestedPropertyExpression(parameter, "Metadata.Block.BlockTime");
                break;
            case "BlockHeight":
                property = GetNestedPropertyExpression(parameter, "Metadata.Block.BlockHeight");
                break;
            case "HolderCount":
                property = GetNestedPropertyExpression(parameter, "HolderCount");
                break;
            case "TransferCount":
                property = GetNestedPropertyExpression(parameter, "TransferCount");
                break;
            case "Symbol":
                property = GetNestedPropertyExpression(parameter, typeof(T) != typeof(TokenInfo) ? "Token.Symbol" : "Symbol");
                break;
            case "FormatAmount":
                property = GetNestedPropertyExpression(parameter, "FormatAmount");
                break;
            case "Address":
                property = GetNestedPropertyExpression(parameter, "Address");
                break;
            case "TransactionId":
                property = GetNestedPropertyExpression(parameter, "TransactionId");
                break;
            case "ChainId":
                property = GetNestedPropertyExpression(parameter, "Metadata.ChainId");
                break;
            default:
                throw new Exception("Invalid order by field");
        }

        var lambda = Expression.Lambda(property, parameter);
        string methodName = sort == SortType.Asc.ToString() ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.Type },
            queryable.Expression, Expression.Quote(lambda));

        return queryable.Provider.CreateQuery<T>(resultExpression);
    }
    
    private static Expression GetNestedPropertyExpression(Expression parameter, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        Expression property = parameter;
        foreach (var prop in properties)
        {
            property = Expression.Property(property, prop);
        }
        return property;
    }
}