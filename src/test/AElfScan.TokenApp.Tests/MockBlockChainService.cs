using AeFinder.Sdk;
using AElf.Contracts.MultiToken;
using Google.Protobuf;
using Volo.Abp.DependencyInjection;

namespace AElfScan.TokenApp;

public class MockBlockChainService : IBlockChainService, ITransientDependency
{
    public async Task<T> ViewContractAsync<T>(string chainId, string contractAddress, string methodName, IMessage parameter) where T : IMessage<T>, new()
    {
        switch (methodName)
        {
            case "GetTokenInfo":
                var result = new T();
                var input = GetTokenInfoInput.Parser.ParseFrom(parameter.ToByteArray());
                result.MergeFrom(new AElf.Contracts.MultiToken.TokenInfo
                {
                    TokenName = input.Symbol + " Token",
                    Symbol = input.Symbol,
                    Decimals = input.Symbol=="ELF"? 8 : 0
                }.ToByteArray());
                return result;
        }
        throw new NotImplementedException();
    }
}