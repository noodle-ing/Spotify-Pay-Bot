// See https://aka.ms/new-console-template for more information

using Quartz;
using Quartz.Impl;
using SpotifyTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
// using Update = SpotifyTelegramBot.Update;             !!!! never use it !!!! -> methods doesnt work 
using Update = Telegram.Bot.Types.Update; 

class Program
{
    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;
    
    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;
    
    static async Task Main()
    {
        _botClient = new TelegramBotClient("6919816985:AAH3l0FCjEMtojvl4HRydn6ia0U6jPo51xc"); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
        _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
        {
            AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
            {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            ThrowPendingUpdates = true, // for fix it use 19 version of library 
        };
        
        using var cts = new CancellationTokenSource();
        
        // UpdateHander - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота
        
        var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
        Console.WriteLine($"{me.FirstName} запущен!");
        
        await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
        
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            var message = update.Message;
            var chat = message.Chat;
            
            
            // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    if (message.Text == "/start")
                    {
                        await botClient.SendTextMessageAsync(
                                chat.Id,  // это обязательное поле
                            $"Этот бот для опалты {chat.Id}"
                            );
                        PaymentReminder();
                        return;
                    }
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    
    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }

    private static async void PaymentReminder()
    {
        IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
        await scheduler.Start();

        // Define a job and link it to the ReminderJob class
        IJobDetail job = JobBuilder.Create<ReminderJob>()
            .WithIdentity("monthlyReminder")
            .Build();

        // Create a trigger to run the job every month on the 1st at 9:00 AM
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("monthlyTrigger")
            .StartNow()
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(15, 2, 50))
            .Build();

        // Schedule the job
        await scheduler.ScheduleJob(job, trigger);

        Console.WriteLine("Press [Enter] to exit...");
    }
    
}