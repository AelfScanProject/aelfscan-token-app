namespace AElfScan.TokenApp;

public class TokenAppConstants
{
    public static string BaseTokenSymbol ="ELF";
    public static Dictionary<string, string> ContractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVV", "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX" }
    };
    
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
    // Token Address、Profit Address、TokenConvert Address
    public static readonly Dictionary<string, List<string>> AddressListMap = new()
    {
        {
            "AELF", new List<string>(){
                "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
                "2ZUgaDqWSh4aJ5s5Ker2tRczhJSNep4bVVfrRBRJTRQdMTbA5W",
                "SietKh9cArYub9ox6E4rU94LrzPad6TB72rCwe3X1jQ5m1C34"
            }
            
        },
        {   
            "tDVV",new List<string>(){
                "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX",
                "2YkY2kjG7dTPJuHcTP3fQyMqat2CMfo7kZoRr7QdejyHHbT4rk"
            }
        }
    };
}