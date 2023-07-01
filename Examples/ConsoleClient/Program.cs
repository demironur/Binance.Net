using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System.Linq;

BinanceClient.SetDefaultOptions(new BinanceClientOptions()
{
    ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"), // <- Provide you API key/secret in these fields to retrieve data related to your account
    LogLevel = LogLevel.Debug
});
BinanceSocketClient.SetDefaultOptions(new BinanceSocketClientOptions()
{
    ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"),
    LogLevel = LogLevel.Debug
});

using (var client = new BinanceClient())
{
    List<decimal> allAverageList = new List<decimal>();
    List<decimal> marketWeekdayOpenAverageList = new List<decimal>();
    List<decimal> marketWeekdayCloseAverageList = new List<decimal>();
    List<decimal> marketWeekendOrWeekDayCloseAverageList = new List<decimal>();

    for (int i = 1; i <= 5; i++)//May
    {
        var startdate = new DateTime(2023, i, 1);
        var enddate = new DateTime(2023, i + 1, 1).AddMinutes(-1);
        var result = await client.SpotApi.ExchangeData.GetKlinesAsync("USDTTRY", KlineInterval.OneHour, startdate, enddate);
        IEnumerable<Binance.Net.Interfaces.IBinanceKline> KlineList = result.Data;

        var allAverage = System.Math.Round(KlineList.Average(x => x.LowPrice), 2);
        allAverageList.Add(allAverage);
        Console.WriteLine("allAverage: " + allAverage + "Start Date: " + KlineList.First().OpenTime + " End Date: " + KlineList.Last().CloseTime);

        var marketWeekdayOpenAverage = System.Math.Round(KlineList.Where(x => x.OpenTime.DayOfWeek != DayOfWeek.Saturday && x.OpenTime.DayOfWeek != DayOfWeek.Sunday &&
        x.OpenTime.Hour >= 6 && x.OpenTime.Hour <= 15).Average(x => x.LowPrice), 2);
        marketWeekdayOpenAverageList.Add(marketWeekdayOpenAverage);
        Console.WriteLine("marketWeekdayOpenAverage: " + marketWeekdayOpenAverage + " Difference:" + (marketWeekdayOpenAverage - allAverage));

        var marketWeekdayCloseAverage = System.Math.Round(KlineList.Where(x => x.OpenTime.DayOfWeek != DayOfWeek.Saturday && x.OpenTime.DayOfWeek != DayOfWeek.Sunday &&
        (x.OpenTime.Hour >= 15 || x.OpenTime.Hour <= 6)).Average(x => x.LowPrice), 2);
        marketWeekdayCloseAverageList.Add(marketWeekdayCloseAverage);
        Console.WriteLine("marketWeekdayCloseAverage: " + marketWeekdayCloseAverage + " Difference:" + (marketWeekdayCloseAverage - allAverage));

        var marketWeekendOrWeekDayCloseAverage = System.Math.Round(KlineList.Where(x => x.OpenTime.DayOfWeek == DayOfWeek.Saturday || x.OpenTime.DayOfWeek == DayOfWeek.Sunday ||
        (x.OpenTime.Hour >= 15 || x.OpenTime.Hour <= 6)).Average(x => x.LowPrice), 2);
        marketWeekendOrWeekDayCloseAverageList.Add(marketWeekendOrWeekDayCloseAverage);
        Console.WriteLine("marketWeekendOrWeekDayCloseAverage: " + marketWeekendOrWeekDayCloseAverage + " Difference:" + (marketWeekendOrWeekDayCloseAverage - allAverage));
    }

    Console.WriteLine("allAverageList: " + allAverageList.Average());
    Console.WriteLine("marketWeekdayOpenAverageList: " + marketWeekdayOpenAverageList.Average());
    Console.WriteLine("marketWeekdayCloseAverageList: " + marketWeekdayCloseAverageList.Average());
    Console.WriteLine("marketWeekendOrWeekDayCloseAverageList: " + marketWeekendOrWeekDayCloseAverageList.Average());

    //var marketOpenAverage = KlineList.Where(x => x.OpenTime.Hour >= 6 && x.OpenTime.Hour <= 15).Average(x => x.LowPrice);
    //Console.WriteLine("marketOpenAverage: " + marketOpenAverage);

    //var marketCloseAverage = KlineList.Where(x => x.OpenTime.Hour >= 15 || x.OpenTime.Hour <= 6).Average(x => x.LowPrice);
    //Console.WriteLine("marketCloseAverage: " + marketCloseAverage);

    //var marketWeekendAverage = KlineList.Where(x => x.OpenTime.DayOfWeek == DayOfWeek.Saturday || x.OpenTime.DayOfWeek == DayOfWeek.Sunday).Average(x => x.LowPrice);
    //Console.WriteLine("marketWeekendAverage: " + marketWeekendAverage);

    //var marketWeekdayAverage = KlineList.Where(x => x.OpenTime.DayOfWeek != DayOfWeek.Saturday && x.OpenTime.DayOfWeek != DayOfWeek.Sunday).Average(x => x.LowPrice);
    //Console.WriteLine("marketWeekdayAverage: " + marketWeekdayAverage);


    //var weekAverage = KlineList.Average(x => x.LowPrice);
    //Console.WriteLine("weekAverage: " + weekAverage);


    //await HandleRequest("Klines", () => client.SpotApi.ExchangeData.GetKlinesAsync("USDTTRY", KlineInterval.TwelveHour), result => string.Join(", ", result..Select(s => s.Name).Take(10)) + " etc");
}



string? read = "";
while (read != "R" && read != "S") 
{
    Console.WriteLine("Run [R]est or [S]ocket example?");
    read = Console.ReadLine();
}

if (read == "R")
{
    using (var client = new BinanceClient())
    {
        await HandleRequest("Symbol list", () => client.SpotApi.ExchangeData.GetExchangeInfoAsync(), result => string.Join(", ", result.Symbols.Select(s => s.Name).Take(10)) + " etc");
        await HandleRequest("BTCUSDT book price", () => client.SpotApi.ExchangeData.GetBookPriceAsync("BTCUSDT"), result => $"Best Ask: {result.BestAskPrice}, Best Bid: {result.BestBidPrice}");
        await HandleRequest("ETHUSDT 24h change", () => client.SpotApi.ExchangeData.GetTickerAsync("ETHUSDT"), result => $"Change: {result.PriceChange}, Change percentage: {result.PriceChangePercent}");
    }
}
else
{
    Console.WriteLine("Press enter to subscribe to BTCUSDT trade stream");
    Console.ReadLine();
    var socketClient = new BinanceSocketClient();
    var subscription = await socketClient.SpotStreams.SubscribeToTradeUpdatesAsync("BTCUSDT", data =>
    {
        Console.WriteLine($"{data.Data.TradeTime}: {data.Data.Quantity} @ {data.Data.Price}");
    });
    if (!subscription.Success)
    {
        Console.WriteLine("Failed to sub: " + subscription.Error);
        Console.ReadLine();
        return;
    }

    subscription.Data.ConnectionLost += () => Console.WriteLine("Connection lost, trying to reconnect..");
    subscription.Data.ConnectionRestored += (t) => Console.WriteLine("Connection restored");

    Console.ReadLine();
    /// Unsubscribe
    await socketClient.UnsubscribeAllAsync();
}

static async Task HandleRequest<T>(string action, Func<Task<WebCallResult<T>>> request, Func<T, string> outputData)
{
    Console.WriteLine("Press enter to continue");
    Console.ReadLine();
    Console.Clear();
    Console.WriteLine("Requesting " + action + " ..");
    var bookPrices = await request();
    if (bookPrices.Success)
        Console.WriteLine($"{action}: " + outputData(bookPrices.Data));
    else
        Console.WriteLine($"Failed to retrieve data: {bookPrices.Error}");
    Console.WriteLine();
}

//var socketClient = new BinanceSocketClient();
//// Spot | Spot market and user subscription methods
//socketClient.Spot.SubscribeToAllBookTickerUpdatesAsync(data =>
//{
//    // Handle data
//});

//// FuturesCoin | Coin-M futures market and user subscription methods
//socketClient.FuturesCoin.SubscribeToAllBookTickerUpdatesAsync(data =>
//{
//    // Handle data
//});

//// FuturesUsdt | USDT-M futures market and user subscription methods
//socketClient.FuturesUsdt.SubscribeToAllBookTickerUpdatesAsync(data =>
//{
//    // Handle data
//});
Console.ReadLine();