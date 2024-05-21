using Orleans.TestingHost;
using AElfScan.TokenApp.TestBase;
using Volo.Abp.Modularity;

namespace AElfScan.TokenApp.Orleans.TestBase;

public abstract class AElfScanTokenAppOrleansTestBase<TStartupModule>:AElfScanTokenAppTestBase<TStartupModule> 
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public AElfScanTokenAppOrleansTestBase()
    {
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}