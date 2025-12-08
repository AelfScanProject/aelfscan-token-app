using AutoMapper;

namespace AElfScan.TokenApp;

public class IndexerMapperBase : Profile
{
    
    protected static string MapLowerCaseString(string value)
    {
        return value?.ToLower();
    }
    
    
    protected static string MapHash(AElf.Types.Hash hash)
    {
        return hash?.ToHex();
    }
    
    protected static string MapAddress(AElf.Types.Address address)
    {
        return address?.ToBase58();
    }
    
    protected static string MapLowerCaseAddress(AElf.Types.Address address)
    {
        return address?.ToBase58().ToLower();
    }
    
    protected static DateTime? MapDateTime(Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
        return timestamp?.ToDateTime();
    }
}