
namespace AElfScan.TokenApp;

public interface ITokenContractAddressProvider
{
    string GetContractAddress(string chainId);
}

public class TokenContractAddressProvider : ITokenContractAddressProvider
{
    private readonly Dictionary<string, string> _contractAddresses = new()
    {
        { "AELF", "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE" },
        { "tDVV", "7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX" }
    };

    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}