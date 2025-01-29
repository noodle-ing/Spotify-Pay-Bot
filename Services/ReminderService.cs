using Quartz;
using Quartz.Impl;
using Telegram.Bot;

namespace SpotifyTelegramBot.Services;

public class ReminderService
{
    private readonly ITelegramBotClient _botClient;
    
    public ReminderService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }
    
    public  async void PaymentReminder()
    {
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        IJobDetail job = JobBuilder.Create<ReminderJob>()
            .WithIdentity("monthlyReminder")
            .UsingJobData("botClient", "6919816985:AAH3l0FCjEMtojvl4HRydn6ia0U6jPo51xc") 
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("monthlyTrigger")
            .StartNow()
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(3, 10, 15))
            .Build();

        await scheduler.ScheduleJob(job, trigger);

        Console.WriteLine("Press [Enter] to exit...");
    }
}