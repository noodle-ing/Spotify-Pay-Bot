using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;

namespace SpotifyTelegramBot;

public class ReminderJob : IJob
{
    private static long _chatId = -4606140584;
    private string jsonPayedUserPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/PayedUsers.json"));
    
    
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            Console.WriteLine("Reminder job executed.");

            var botToken = context.MergedJobDataMap.GetString("botClient");
            if (string.IsNullOrEmpty(botToken))
            {
                throw new Exception("Bot token is missing in JobDataMap.");
            }

            var botClient = new TelegramBotClient(botToken);
            
            await botClient.SendTextMessageAsync(
                _chatId,
                "Всем привет! Время оплаты подписки \n" +
                "Скидываем как обычно 700 тг"
            );
            CleanPayedList(jsonPayedUserPath);
            ScheduleFollowUp(botToken);
            Console.WriteLine("Reminder sent successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ReminderJob: {ex.Message}");
        }
    }
    
    private void CleanPayedList(string filePath) // transfer to user server 
    {
        List<User> cleanPayList = new List<User>();
        string json = JsonConvert.SerializeObject(cleanPayList, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    
    
    private static async void ScheduleFollowUp(string botToken)
    {
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        IJobDetail followUpJob = JobBuilder.Create<ForgetReminder>()
            .WithIdentity("followUpReminder")
            .UsingJobData("botClient", botToken)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("followUpTrigger")
            .StartAt(DateTimeOffset.Now.AddDays(3)) 
            .Build();

        await scheduler.ScheduleJob(followUpJob, trigger);

        Console.WriteLine("Follow-up job scheduled.");
    }
}