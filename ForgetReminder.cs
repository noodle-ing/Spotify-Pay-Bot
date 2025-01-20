using Quartz;
using Telegram.Bot;

namespace SpotifyTelegramBot;

public class ForgetReminder : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Console.WriteLine("Follow-up job executed.");

            var botToken = context.MergedJobDataMap.GetString("botClient");
            if (string.IsNullOrEmpty(botToken))
            {
                throw new Exception("Bot token is missing in JobDataMap.");
            }

            var botClient = new TelegramBotClient(botToken);

            string subscribersFilePath = "C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\Subscribers.json";
            string payedUsersFilePath = "C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\PayedUsers.json";

            // Load subscribers and payed users
            var allUsers = Program.LoadUsers(subscribersFilePath);
            var payedUsers = Program.LoadUsers(payedUsersFilePath);

            // Find users who haven't paid
            var nonPayers = allUsers.Where(user => !payedUsers.Any(p => p.Id == user.Id)).ToList();

            foreach (var user in nonPayers)
            {
                await botClient.SendTextMessageAsync(
                    user.Id,
                    "Привет! Мы заметили, что ты еще не оплатил подписку на Spotify. Пожалуйста, сделай это в ближайшее время."
                );
                Console.WriteLine($"Direct message sent to user: {user.Id}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in FollowUpJob: {ex.Message}");
        }
    }
}