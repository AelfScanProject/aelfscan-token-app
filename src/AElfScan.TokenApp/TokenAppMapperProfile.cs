using AElfScan.TokenApp.Entities;
using AElfScan.TokenApp.GraphQL;
using AElf;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AutoMapper;

namespace AElfScan.TokenApp;

public class TokenAppMapperProfile : IndexerMapperBase
{
    public TokenAppMapperProfile()
    {
        // Common
        CreateMap<Hash, string>().ConvertUsing(s => s == null ? string.Empty : s.ToHex());
        CreateMap<Address, string>().ConvertUsing(s => s == null ? string.Empty : s.ToBase58());

        // TokenInfo
        CreateMap<TokenCreated, Entities.TokenInfo>()
            .ForMember(d => d.LowerCaseSymbol,
                opt => opt.MapFrom(s => MapLowerCaseString(s.Symbol)))
            .ForMember(d => d.ExternalInfo,
                opt => opt.MapFrom(s => s.ExternalInfo.Value.ToDictionary(o => o.Key, o => o.Value)))
            .ForMember(d => d.IssueChainId,
                opt => opt.MapFrom(s =>
                    s.IssueChainId == 0 ? null : ChainHelper.ConvertChainIdToBase58(s.IssueChainId)));
        CreateMap<Entities.TokenInfo, TokenInfoDto>()
            .ForMember(d => d.ExternalInfo,
                opt => opt.MapFrom(s => s.ExternalInfo == null
                    ? new List<TokenExternalInfoDto>()
                    : s.ExternalInfo.Select(o => new TokenExternalInfoDto
                    {
                        Key = o.Key,
                        Value = o.Value
                    }).ToList()));
        CreateMap<Entities.TokenInfo, TokenBase>()
            .ForMember(d => d.LowerCaseSymbol,
                opt => opt.MapFrom(s => MapLowerCaseString(s.Symbol)));
        CreateMap<TokenBase, TokenBaseDto>();

        // Transfer
        CreateMap<Transferred, TransferInfo>()
            .ForMember(d => d.LowerCaseFrom, opt => opt.MapFrom(s => MapLowerCaseAddress(s.From)))
            .ForMember(d => d.LowerCaseTo, opt => opt.MapFrom(s => MapLowerCaseAddress(s.To)));
        CreateMap<CrossChainTransferred, TransferInfo>()
            .ForMember(d => d.IssueChainId,
                opt => opt.MapFrom(s =>
                    s.IssueChainId == 0 ? null : ChainHelper.ConvertChainIdToBase58(s.IssueChainId)))
            .ForMember(d => d.ToChainId,
                opt => opt.MapFrom(s => s.ToChainId == 0 ? null : ChainHelper.ConvertChainIdToBase58(s.ToChainId)))
            .ForMember(d => d.LowerCaseFrom, opt => opt.MapFrom(s => MapLowerCaseAddress(s.From)))
            .ForMember(d => d.LowerCaseTo, opt => opt.MapFrom(s => MapLowerCaseAddress(s.To)));
        CreateMap<CrossChainReceived, TransferInfo>()
            .ForMember(d => d.IssueChainId,
                opt => opt.MapFrom(s =>
                    s.IssueChainId == 0 ? null : ChainHelper.ConvertChainIdToBase58(s.IssueChainId)))
            .ForMember(d => d.FromChainId,
                opt => opt.MapFrom(s => s.FromChainId == 0 ? null : ChainHelper.ConvertChainIdToBase58(s.FromChainId)))
            .ForMember(d => d.LowerCaseFrom, opt => opt.MapFrom(s => MapLowerCaseAddress(s.From)))
            .ForMember(d => d.LowerCaseTo, opt => opt.MapFrom(s => MapLowerCaseAddress(s.To)));
        CreateMap<TransferInfo, TransferInfoDto>()
            .ForMember(d => d.ExtraProperties,
                opt => opt.MapFrom(s => s.ExtraProperties == null
                    ? new List<ExtraProperty>()
                    : s.ExtraProperties.Select(o => new ExtraProperty
                    {
                        Key = o.Key,
                        Value = o.Value
                    }).ToList()));
        CreateMap<Issued, TransferInfo>()
            .ForMember(d => d.LowerCaseTo, opt => opt.MapFrom(s => MapLowerCaseAddress(s.To)));

        CreateMap<Burned, TransferInfo>()
            .ForMember(d => d.LowerCaseFrom, opt => opt.MapFrom(s => MapLowerCaseAddress(s.Burner)));

        // Account Token
        CreateMap<AccountToken, AccountTokenDto>();

        // Account Info
        CreateMap<AccountInfo, AccountInfoDto>();
    }
}