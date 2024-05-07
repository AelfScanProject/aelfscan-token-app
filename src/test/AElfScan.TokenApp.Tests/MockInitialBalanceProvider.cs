using Volo.Abp.DependencyInjection;

namespace AElfScan.TokenApp;

public class MockInitialBalanceProvider : IInitialBalanceProvider, ISingletonDependency
{
    private readonly Dictionary<string, Dictionary<long, List<string>>> _initialBalances = new();

    public MockInitialBalanceProvider()
    {
        InitBalance();
    }

    public List<string> GetInitialBalances(string chainId, long blockHeight)
    {
        if (_initialBalances.TryGetValue(chainId, out var chainBalances) &&
            chainBalances.TryGetValue(blockHeight, out var balances))
        {
            return balances;
        }

        return new List<string>();
    }

    private void InitBalance()
    {
        var balance = new Dictionary<long, List<string>>
        {
            {
                99, new List<string>
                {
                    "Address1,ELF,100000000",
                    "Address2,ELF,200000000",
                    "Address3,NFT-0,1",
                    "Address4,NFT-1,100",
                    "Address4,NFT-1,200",
                    "Address1,NFT-2,300"
                }
            },
            {
                99, new List<string>
                {
                    "Address1,ELF,100000000",
                    "Address2,ELF,200000000",
                    "Address3,NFT-0,1",
                    "Address4,NFT-1,100",
                    "Address4,NFT-1,200",
                    "Address1,NFT-2,300"
                }
            }
        };

        _initialBalances["AELF"] = balance;
    }
}