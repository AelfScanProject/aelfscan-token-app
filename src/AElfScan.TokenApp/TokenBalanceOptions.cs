namespace AElfScan.TokenApp;

public class TokenBalanceOptions
{
    public Dictionary<string, TokenBalanceOption> InitBalances { get; set; } = new();
}

public class TokenBalanceOption
{
    public long InitStartHeight { get; set; }
    public long InitEndHeight { get; set; }
    public long InitIntervalHeight { get; set; } = 100;
    public string InitFilePath { get; set; }
    public long StartIndexHeight { get; set; } = 1;
}