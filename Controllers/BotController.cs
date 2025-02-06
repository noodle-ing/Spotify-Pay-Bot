using SpotifyTelegramBot.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;

namespace SpotifyTelegramBot.Controllers;

public class BotController
{
    private static ITelegramBotClient _botClient;
    private static Update _update;
    private static CancellationToken _cancellationToken;
    private UserService _userService;
    private static ReminderService _reminderService = new ReminderService(_botClient);
    private BotService _botService = new BotService(_botClient, _update, _reminderService);

    public BotController(ITelegramBotClient botClient, CancellationToken cancellationToken, Update update)
    {
        _botClient = botClient;
        _cancellationToken = cancellationToken;
        _update = update;
    }
    
    public async Task UpdateHandler()
    {
        try
        {
            _botService.Command();
            _userService.UserCommandHandler();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
   
    
    private static Task ErrorHandler( Exception error)
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