using Shouldly;
using Xunit;

namespace AElfScan.TokenApp.Processors;

public class TokenProcessorBaseTests: TokenContractAppTestBase
{
    private readonly TransferredProcessor _transferredProcessor;

    public TokenProcessorBaseTests()
    {
        _transferredProcessor = GetRequiredService<TransferredProcessor>();
    }
    
    [Fact]
    public async Task GetContractAddressTest()
    {
        var contractAddress = _transferredProcessor.GetContractAddress(ChainId);
        contractAddress.ShouldBe("JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE");
        
        contractAddress = _transferredProcessor.GetContractAddress("tDVV");
        contractAddress.ShouldBe("7RzVGiuVWkvL4VfVHdZfQF2Tri3sgLe9U991bohHFfSRZXuGX");
    }
}