
namespace AElfScan.TokenApp;

public interface ITokenContractAddressProvider
{
    string GetContractAddress(string chainId);
}

public class TokenContractAddressProvider : ITokenContractAddressProvider
{
    private readonly Dictionary<string, string> _contractAddresses = TokenAppConstants.ContractAddresses;
    
    public string GetContractAddress(string chainId)
    {
        return _contractAddresses[chainId];
    }
}