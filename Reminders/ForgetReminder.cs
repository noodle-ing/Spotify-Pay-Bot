using Quartz;
using SpotifyTelegramBot.Maping;
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

            string  filePathSubscribers = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/Subscribers.json"));
            string  filePathPayedUser = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/PayedUsers.json"));

            UserMaping userMaping = new UserMaping();
            var allUsers = userMaping.LoadUsers(filePathSubscribers);
            var payedUsers = userMaping.LoadUsers(filePathPayedUser);

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