
using AeFinder.App.TestBase;
using AElfScan.TokenApp.Processors;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace AElfScan.TokenApp;

[DependsOn(
    typeof(AeFinderAppTestBaseModule),
    typeof(TokenAppModule))]
public class AElfScanTokenAppTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AeFinderAppEntityOptions>(options => { options.AddTypes<TokenAppModule>(); });

        context.Services.AddSingleton<BurnedProcessor>();
        context.Services.AddSingleton<ChainPrimaryTokenSymbolSetProcessor>();
        context.Services.AddSingleton<CrossChainReceivedProcessor>();
        context.Services.AddSingleton<CrossChainTransferredProcessor>();
        context.Services.AddSingleton<IssuedProcessor>();
        context.Services.AddSingleton<RentalChargedProcessor>();
        context.Services.AddSingleton<ResourceTokenClaimedProcessor>();
        context.Services.AddSingleton<TokenCreatedProcessor>();
        context.Services.AddSingleton<TransactionFeeChargedProcessor>();
        context.Services.AddSingleton<TransactionFeeClaimedProcessor>();
        context.Services.AddSingleton<TransferredProcessor>();
    }
    
}