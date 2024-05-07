using AElfScan.TokenApp.Entities;

namespace AElfScan.TokenApp;

public class TokenSymbolHelper
{
    public const char NFTSymbolSeparator = '-';
    public const string CollectionSymbolSuffix = "0";

    public static SymbolType GetSymbolType(string symbol)
    {
        var words = symbol.Split(NFTSymbolSeparator);
        if (words.Length == 1) return SymbolType.Token;
        return words[1] == CollectionSymbolSuffix ? SymbolType.NftCollection : SymbolType.Nft;
    }

    public static string GetCollectionSymbol(string symbol)
    {
        var words = symbol.Split(NFTSymbolSeparator);
        return words.Length == 1 || words[1] == CollectionSymbolSuffix
            ? null
            : $"{words[0]}{NFTSymbolSeparator}{CollectionSymbolSuffix}";
    }
}