using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.CSharp.Core;
using AElfScan.TokenApp.Helper;
using Newtonsoft.Json;
using Volo.Abp.ObjectMapping;

namespace AElfScan.TokenApp.Processors;

public abstract class TokenProcessorBase<TEvent> : LogEventProcessorBase<TEvent> where TEvent : IEvent<TEvent>, new()
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();

    public ITokenContractAddressProvider ContractAddressProvider { get; set; }


    public override string GetContractAddress(string chainId)
    {
        return ContractAddressProvider.GetContractAddress(chainId);
    }

    protected async Task<TokenInfo> GetTokenAsync(string chainId, string symbol)
    {
        var tokenId = IdGenerateHelper.GetId(chainId, symbol);
        return await GetEntityAsync<TokenInfo>(tokenId);
    }

    protected async Task<AccountInfo> GetAccountInfoAsync(string chainId, string address)
    {
        var accountId = IdGenerateHelper.GetId(chainId, address);
        return await GetEntityAsync<AccountInfo>(accountId);
    }

    protected async Task<AccountToken> GetAccountTokenAsync(string chainId, string address, string symbol)
    {
        var accountTokenId = IdGenerateHelper.GetId(chainId, address, symbol);
        return await GetEntityAsync<AccountToken>(accountTokenId);
    }

    protected async Task IncreaseTokenInfoTransferCountAsync(LogEventContext context, string symbol, bool ignoreCollection = true)
    {
        var token = await GetTokenAsync(context.ChainId, symbol);
        if (ignoreCollection && token.Type == SymbolType.NftCollection)
        {
            return;
        }
        token.TransferCount += 1;
        await SaveEntityAsync(token);
        if (token.Type == SymbolType.Nft)
        {
            await IncreaseTokenInfoTransferCountAsync(context, token.CollectionSymbol,false);
        }
    }

    protected async Task IncreaseAccountTokenTransferCountAsync(LogEventContext context, string address, string symbol)
    {
        var accountToken = await GetAccountToken(context, symbol, address);
        accountToken.TransferCount += 1;
        await SaveEntityAsync(accountToken);
        if (accountToken.Token.Type == SymbolType.Nft)
        {
            var accountCollection = await GetAccountCollection(context, accountToken.Token.CollectionSymbol, address);
            accountCollection.TransferCount += 1;
            await SaveEntityAsync(accountCollection);
        }
    }

    protected async Task IncreaseAccountTransferCountAsync(LogEventContext context, string address)
    {
        var accountInfo = await GetAccountInfoAsync(context.ChainId, address);
        if (accountInfo == null)
        {
            accountInfo = new AccountInfo
            {
                Id = IdGenerateHelper.GetId(context.ChainId, address),
                Address = address
            };
        }

        accountInfo.TransferCount += 1;
        await SaveEntityAsync(accountInfo);
    }

    protected async Task IncreaseAccountTransferCountAsync(LogEventContext context, string address, string symbol)
    {
        await IncreaseAccountTokenTransferCountAsync(context, address, symbol);
        await IncreaseAccountTransferCountAsync(context, address);
    }

    protected async Task ChangeTokenInfoHolderCountAsync(LogEventContext context, string symbol, long changeValue)
    {
        var token = await GetTokenAsync(context.ChainId, symbol);
        token.HolderCount += changeValue;
        await SaveEntityAsync(token);
    }

    protected async Task ChangeAccountHoldingCountAsync(LogEventContext context, string address, long changeValue)
    {
        var accountInfo = await GetAccountInfoAsync(context.ChainId, address);
        if (accountInfo == null)
        {
            accountInfo = new AccountInfo
            {
                Id = IdGenerateHelper.GetId(context.ChainId, address),
                Address = address
            };
        }

        accountInfo.TokenHoldingCount += changeValue;
        await SaveEntityAsync(accountInfo);
    }

    protected async Task ModifyBalanceAsync(LogEventContext context, string symbol, string address, long amount)
    {
        await RecordFirstNftInfoAsync(context, symbol, address);

        var beforeDate = DateTimeHelper.GetBeforeDate(context.Block.BlockTime);
        if (TokenAppConstants.InitialBalanceEndHeight.TryGetValue(context.ChainId, out var h) &&
            context.Block.BlockHeight > h)
        {
            var id = IdGenerateHelper.GetId(context.ChainId, beforeDate);
            var beforeData = await GetEntityAsync<DailyHolderInfo>(id);
            var token = await GetTokenAsync(context.ChainId, "ELF");
            if (beforeData == null && token != null)
            {
                var dailyHolder = new DailyHolderInfo()
                {
                    Id = id,
                    ChainId = context.ChainId,
                    Count = token.HolderCount,
                    DateStr = beforeDate
                };
                Logger.LogInformation("ModifyBalanceAsync dailyHolder:{p}",JsonConvert.SerializeObject(dailyHolder));
                Logger.LogInformation("Add daily holder:chainId:{c},date:{d},count:{c}", context.ChainId, beforeDate,
                    token.HolderCount);

                await SaveEntityAsync(dailyHolder);
            }
        }


        if (TokenAppConstants.StartProcessBalanceEventHeight.TryGetValue(context.ChainId, out var height) &&
            context.Block.BlockHeight < height)
        {
            return;
        }


        await ModifyBalanceAndChangeHoldingCountAsync(context, symbol, address, amount);
    }

    private async Task RecordFirstNftInfoAsync(LogEventContext context, string symbol, string address)
    {
        var token = await GetTokenAsync(context.ChainId, symbol);
        if (token?.Type != SymbolType.Nft)
            return;

        var accountToken = await GetAccountToken(context, symbol, address);

        if (!accountToken.FirstNftTransactionId.IsNullOrWhiteSpace())
        {
            return;
        }

        accountToken.FirstNftTransactionId = context.Transaction.TransactionId;
        accountToken.FirstNftTime = context.Block.BlockTime;
        Logger.LogInformation("RecordFirstNftInfoAsync accountToken:{p}",JsonConvert.SerializeObject(accountToken));
        await SaveEntityAsync(accountToken);
    }

    protected async Task<AccountToken> GetAccountToken(LogEventContext context, string symbol, string address)
    {
        var accountTokenId = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var accountToken = await GetEntityAsync<AccountToken>(accountTokenId);
        if (accountToken == null)
        {
            var token = await GetTokenAsync(context.ChainId, symbol);
            accountToken = new AccountToken
            {
                Id = accountTokenId,
                Address = address,
                Token = ObjectMapper.Map<TokenInfo, TokenBase>(token),
                LowerCaseAddress = address.ToLower()
            };
        }

        return accountToken;
    }
    
    protected async Task<AccountCollection> GetAccountCollection(LogEventContext context, string symbol, string address)
    {
        var accountTokenId = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var accountCollection = await GetEntityAsync<AccountCollection>(accountTokenId);
        if (accountCollection == null)
        {
            var token = await GetTokenAsync(context.ChainId, symbol);
            accountCollection = new AccountCollection
            {
                Id = accountTokenId,
                Address = address,
                Token = ObjectMapper.Map<TokenInfo, TokenBase>(token),
                LowerCaseAddress = address.ToLower()
            };
        }

        return accountCollection;
    }

    private async Task ModifyBalanceAndChangeHoldingCountAsync(LogEventContext context, string symbol, string address,
        long amount)
    {
        var accountToken = await GetAccountToken(context, symbol, address);
        var originalBalance = accountToken.Amount;
        accountToken.Amount += amount;
        accountToken.FormatAmount = accountToken.Amount / (decimal)Math.Pow(10, accountToken.Token.Decimals);

        await SaveEntityAsync(accountToken);
        if (accountToken.Token.Type == SymbolType.NftCollection)
        {
            return;
        }
        switch (originalBalance)
        {
            case > 0 when accountToken.Amount == 0:
                await ChangeAccountHoldingCountAsync(context, address, -1);
                await ChangeTokenInfoHolderCountAsync(context, symbol, -1);
                break;
            case 0 when accountToken.Amount > 0:
                await ChangeAccountHoldingCountAsync(context, address, 1);
                await ChangeTokenInfoHolderCountAsync(context, symbol, 1);
                break;
        }
        if (accountToken.Token.Type == SymbolType.Nft)
        {
          await  ChangeCollectionBalanceAndChangeHoldingCountAsync(context, TokenSymbolHelper.GetCollectionSymbol(symbol),
                address, amount/ (decimal)Math.Pow(10, accountToken.Token.Decimals));
        }
    }

    private async Task ChangeCollectionBalanceAndChangeHoldingCountAsync(LogEventContext context, string symbol, string address,
        decimal amount)
    {

        decimal originalBalance = 0;
        var accountTokenId = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var accountToken = await GetEntityAsync<AccountCollection>(accountTokenId);
        if (accountToken == null)
        {
            var token = await GetTokenAsync(context.ChainId, symbol);
            accountToken = new AccountCollection()
            {
                Id = accountTokenId,
                Address = address,
                Token = ObjectMapper.Map<TokenInfo, TokenBase>(token),
                LowerCaseAddress = address.ToLower(),
                FormatAmount = amount 
            };
        }
        else
        {
            originalBalance = accountToken.FormatAmount;
            accountToken.FormatAmount += amount;
        }
        await SaveEntityAsync(accountToken);
        switch (originalBalance)
        {
            case > 0 when accountToken.FormatAmount == 0:
                await ChangeTokenInfoHolderCountAsync(context, symbol, -1);
                break;
            case 0 when accountToken.FormatAmount > 0:
                await ChangeTokenInfoHolderCountAsync(context, symbol, 1);
                break;
        }
        
    }

    protected async Task AddTransferAsync(TransferInfo transferInfo, LogEventContext context)
    {
        transferInfo.Id = Guid.NewGuid().ToString();
        transferInfo.FormatAmount = transferInfo.Amount / (decimal)Math.Pow(10, transferInfo.Token.Decimals);
        transferInfo.TransactionId = context.Transaction.TransactionId;
        transferInfo.ExtraProperties = context.Transaction.ExtraProperties;
        transferInfo.Status = context.Transaction.Status.ToString();
        await SaveEntityAsync(transferInfo);
    }
    
    protected async Task ChangeCollectionItemCountAsync(LogEventContext context, TokenInfo tokenInfo, long changeValue)
    {
        if (tokenInfo.Type == SymbolType.Nft)
        {
            var collection = await GetTokenAsync(context.ChainId, tokenInfo.CollectionSymbol);
            collection.ItemCount += changeValue/(decimal)Math.Pow(10, tokenInfo.Decimals);
            await SaveEntityAsync(collection);
        }
    }
}