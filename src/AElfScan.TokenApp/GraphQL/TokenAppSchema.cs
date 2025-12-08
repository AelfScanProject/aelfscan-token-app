using AeFinder.Sdk;

namespace AElfScan.TokenApp.GraphQL;

public class TokenAppSchema : AppSchema<Query>
{
    public TokenAppSchema(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
}