using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using AElfScan.TokenApp.Entities;
using AElf.CSharp.Core;
using AElfScan.TokenApp.Helper;
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

    protected async Task IncreaseTokenInfoTransferCountAsync(LogEventContext context, string symbol)
    {
        var token = await GetTokenAsync(context.ChainId, symbol);
        token.TransferCount += 1;
        await SaveEntityAsync(token);

        if (token.Type == SymbolType.Nft)
        {
            await IncreaseTokenInfoTransferCountAsync(context, token.CollectionSymbol);
        }
    }

    protected async Task IncreaseAccountTokenTransferCountAsync(LogEventContext context, string address, string symbol)
    {
        var accountToken = await GetAccountTokenAsync(context.ChainId, address, symbol);
        if (accountToken == null)
        {
            var token = await GetTokenAsync(context.ChainId, symbol);
            accountToken = new AccountToken
            {
                Id = IdGenerateHelper.GetId(context.ChainId, address, symbol),
                Address = address,
                Token = ObjectMapper.Map<TokenInfo, TokenBase>(token)
            };
        }

        accountToken.TransferCount += 1;

        if (accountToken.Token.Type == SymbolType.Nft)
        {
            await IncreaseAccountTokenTransferCountAsync(context, address, accountToken.Token.CollectionSymbol);
        }

        await SaveEntityAsync(accountToken);
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

    protected async Task ChangeTokenInfoHolderCountAsync(LogEventContext context, string symbol, long changeValue,
        bool ignoreCollection = true)
    {
        var token = await GetTokenAsync(context.ChainId, symbol);
        if (ignoreCollection && token.Type == SymbolType.NftCollection)
        {
            return;
        }

        token.HolderCount += changeValue;
        await SaveEntityAsync(token);

        if (token.Type == SymbolType.Nft)
        {
            await ChangeTokenInfoHolderCountAsync(context, token.CollectionSymbol, changeValue, false);
        }
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

        var accountTokenId = IdGenerateHelper.GetId(context.ChainId, address, symbol);
        var accountToken = await GetEntityAsync<AccountToken>(accountTokenId);
        if (accountToken == null)
        {
            accountToken = new AccountToken
            {
                Id = accountTokenId,
                Address = address,
                Token = ObjectMapper.Map<TokenInfo, TokenBase>(token),
            };
        }

        if (!accountToken.FirstNftTransactionId.IsNullOrWhiteSpace())
        {
            return;
        }

        accountToken.FirstNftTransactionId = context.Transaction.TransactionId;
        accountToken.FirstNftTime = context.Block.BlockTime;
        await SaveEntityAsync(accountToken);
    }

    private async Task ModifyBalanceAndChangeHoldingCountAsync(LogEventContext context, string symbol, string address,
        long amount)
    {
        var originalBalance = 0L;
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
                Amount = amount
            };
        }
        else
        {
            originalBalance = accountToken.Amount;
            accountToken.Amount += amount;
        }

        accountToken.FormatAmount = accountToken.Amount / (decimal)Math.Pow(10, accountToken.Token.Decimals);

        await SaveEntityAsync(accountToken);

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
}