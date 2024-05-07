namespace AElfScan.TokenApp;

public class TokenAppConstants
{
    public static Dictionary<string,long> InitialBalanceEndHeight = new Dictionary<string, long>
    {
        { "AELF", 4100 },
        { "tDVV", 5500 }
    };
    public static Dictionary<string,long> StartProcessBalanceEventHeight = new Dictionary<string, long>
    {
        { "AELF", 193837741 },
        { "tDVV", 182214194 }
    };
}