using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SpotifyTelegramBot;

public class ReminderJob : IJob
{
    private readonly ITelegramBotClient _botClient;

    // Constructor to inject the bot client
    public ReminderJob(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    // Execute method expected by Quartz
    public async Task Execute(IJobExecutionContext context)
    {
        // Retrieve chatId dynamically or from your context if needed
        long chatId = 1388592896;  // Replace with a valid chat ID or logic to get it

        Console.WriteLine($"Reminder sent! Time: {DateTime.Now}");

        // Send reminder message
        await _botClient.SendTextMessageAsync(
            chatId, // Chat ID where the message should be sent
            "Время отправить Алтынай 700 тг"
        );
    }
    
}