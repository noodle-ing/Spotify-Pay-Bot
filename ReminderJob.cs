using Quartz;
using Telegram.Bot;

namespace SpotifyTelegramBot;

public class ReminderJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Console.WriteLine("Reminder job executed.");

            // Retrieve the bot client from JobDataMap
            var botToken = context.MergedJobDataMap.GetString("botClient");
            if (string.IsNullOrEmpty(botToken))
            {
                throw new Exception("Bot token is missing in JobDataMap.");
            }

            // Create a new bot client instance
            var botClient = new TelegramBotClient(botToken);

            long chatId = -4606140584 ; // Replace with actual chat ID

            // Send the reminder
            await botClient.SendTextMessageAsync(
                chatId,
                "Всем привет! Время оплаты подписки \n" +
                "Скидываем как обычно 700 тг"
            );

            Console.WriteLine("Reminder sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ReminderJob: {ex.Message}");
        }
    }
}