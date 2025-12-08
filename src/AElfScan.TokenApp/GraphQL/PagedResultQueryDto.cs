namespace AElfScan.TokenApp.GraphQL;

public class PagedResultQueryDto : OrderInfo
{
    public static int DefaultMaxResultCount { get; set; } = 10;
    public static int MaxMaxResultCount { get; set; } = 1000;
    public int SkipCount { get; set; } = 0;
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    public List<OrderInfo> OrderInfos { get; set; }
    public List<string> SearchAfter { get; set; }
    
    public virtual void Validate()
    {
        if (MaxResultCount > MaxMaxResultCount)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxResultCount),
                $"Max allowed value for {nameof(MaxResultCount)} is {MaxMaxResultCount}.");
        }
    }

    //For compatibility
    public List<OrderInfo> GetAdaptableOrderInfos()
    {
        if (OrderBy.IsNullOrEmpty())
        {
            return OrderInfos;
        }

        return new List<OrderInfo>
        {
            new()
            {
                OrderBy = OrderBy,
                Sort = Sort
            }
        };
    }
}

public enum SortType
{
    Asc,
    Desc
}

public class OrderInfo
{
    public string OrderBy { get; set; }
    
    public string Sort { get; set; }
}