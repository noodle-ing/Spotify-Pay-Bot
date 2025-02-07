// See https://aka.ms/new-console-template for more information

using SpotifyTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

using SpotifyTelegramBot.Controllers;
using Update = Telegram.Bot.Types.Update; 
class Program
{
    private static ITelegramBotClient _botClient;
    private static ReceiverOptions _receiverOptions;

    private static List<User> spotifyUsers = new ();
    private static List<User> payedUsers = new ();
    private static Update _update = new Update();
    private static readonly HttpClient client = new();
    
    static async Task Main()
    {
        
        _botClient = new TelegramBotClient("6919816985:AAH3l0FCjEMtojvl4HRydn6ia0U6jPo51xc"); 
        _receiverOptions = new ReceiverOptions 
        {
            AllowedUpdates = new[]
            {
                UpdateType.Message, 
            },
            ThrowPendingUpdates = false  
        };

        using var cts = new CancellationTokenSource();
        var botController = new BotController(_botClient, cts.Token, _update);  

        _botClient.StartReceiving(
            botController.UpdateHandler,  
            ErrorHandler,
            _receiverOptions,
            cts.Token);

        
        var me = await _botClient.GetMeAsync(); 
        Console.WriteLine($"{me.FirstName} is running!");
        
        await Task.Delay(Timeout.Infinite);
    }
    
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };
    
        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}