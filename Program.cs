// See https://aka.ms/new-console-template for more information

using Quartz;
using Quartz.Impl;
using SpotifyTelegramBot;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using System.Net.Http;
using Newtonsoft.Json;
using Update = Telegram.Bot.Types.Update; 
class Program
{
    private static ITelegramBotClient _botClient;
    
    private static ReceiverOptions _receiverOptions;

    private static List<User> spotifyUsers = new ();
    private static List<User> payedUsers = new ();
    
    private static readonly HttpClient client = new();
    
    public static string  filePathSubscribers = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/Subscribers.json"));
    public static string  filePathPayedUser = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/PayedUsers.json"));
    
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
        
        
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); 
        
        var me = await _botClient.GetMeAsync(); 
        Console.WriteLine($"{me.FirstName} is running!");
        
        await Task.Delay(Timeout.Infinite); 
        
    }
    
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            var message = update.Message;
            var chat = message.Chat;
            switch (update.Type)
            {
                case UpdateType.Message:
                {
                    if (message.Text == "/start")
                    {
                        SendWelcomMessage(botClient, chat.Id);
                        return;
                    }
                    if (message.Text == "/payed")
                    {
                        var user = message.From;
                        long userId = user.Id;
                        var userName = user.Username;
                        PaymentMessage(botClient, userId, chat.Id, userName);
                        return;
                    }

                    if (message.Text == "/registerNewUser")
                    {
                        var newUser = message.From;
                        long userId = newUser.Id;
                        NewUserRegistration(botClient, newUser.Username, userId, chat.Id);
                        return;

                    }
                    if (message.Text == "/setReminder")
                    {
                        RemindSender(botClient, chat.Id);
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

    private static bool CheckingUserMembership(long userId) //
    {
        spotifyUsers =
            LoadUsers(filePathSubscribers);
        foreach (var user in spotifyUsers)
        {
            if (userId == user.Id)
            { 
                return true;
            }
        }
        return false;
    }

    private static bool UserNeedToPay(long userId) //
    {
        payedUsers = LoadUsers(filePathPayedUser);
        foreach (var user in payedUsers)
        {
            if (userId == user.Id)
            {
                return false;
            }
        }
        return true;
    }
    
    private static bool NeedToRegister(long userId) //
    {
        spotifyUsers =
            LoadUsers(filePathSubscribers); 
        foreach (var user in spotifyUsers)
        {
            if (userId== user.Id)
            {
                return false;
            }
        }
        return true;
    }
    
    
    private static User RegisterNewUser(long userId) //
    {
        User newRegisterUser = new User();
        newRegisterUser.Id = userId;
        return newRegisterUser; 
    }
    
    static void SaveUsers(string filePath, List<User> users) //
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    
    public static List<User> LoadUsers(string filePath) //
    {
        if (!File.Exists(filePath))
        {
            return new List<User>();
        }
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }

    private static async Task SendWelcomMessage(ITelegramBotClient botClient, long chatId)
    {
        await botClient.SendTextMessageAsync(
            chatId, 
            $"Всем привет это бот для ежемесячного \n" +
            $"напоминания об оплате подписки Spotify.\n" +
            $"Каждый месяц 3 числа я буду отпарвлять уведомления в группу" +
            $" о необходимости оплаты.\n" +
            $"\n" +
            $"Для начла работы бота: \n" +
            $"\n" +
            $"1) Активируйте бота у себя в личных сообщениях просто нажав на start\n" +
            $"\n" +
            $"2) Подпишитесь на ежемесячные уведомления \n" +
            $"\n" +
            $"3) Каждый раз после оплаты не забывайте нажимать на кнопку оплатил \n" +
            $"Если забудете это сделать то бот отправт в лс уведомление о том что вы забыли оплатить"
        );
    }

    private static async Task PaymentMessage(ITelegramBotClient botClient, long userId, long chatId, string userName) 
    {
        if (CheckingUserMembership(userId))
        {
            if (UserNeedToPay(userId))
            {
                payedUsers.Add(RegisterNewUser(userId));
                SaveUsers(filePathPayedUser, payedUsers);
                Console.WriteLine($"{userId} добавлен в список оплаченных");
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"пользователь {userName} оплатил!"
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    $"пользователь {userName} уже оплатил"
                );
            }
            
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                $"пользователю {userName} сперва нужно зарегестрироватся"
            );
        }
    }
    
    private static async Task NewUserRegistration(ITelegramBotClient botClient,string userName, long userId, long chatId) //
    {
        if (spotifyUsers.Count < 5)
        {
            if (NeedToRegister(userId))
            {
                spotifyUsers.Add(RegisterNewUser(userId));
                SaveUsers(filePathSubscribers, spotifyUsers);
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Новый пользователь зарегестрирован"
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId,
                    "Данный пользователь уже зарегестрирован!"
                );
            }
        }
        else
        {
            await botClient.SendTextMessageAsync(
                chatId,
                "Невозможно зарегестрировать нового пользователя \n" +
                "достигнуто максимальное количество участников подписки"
            );
        }
    }

    private static async Task RemindSender(ITelegramBotClient botClient, long chatId)
    {
        await botClient.SendTextMessageAsync(
            chatId,  
            $"Ежемесячные уведомления включены"
        );
        PaymentReminder();
    }
}