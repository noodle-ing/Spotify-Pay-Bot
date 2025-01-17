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
// using Update = SpotifyTelegramBot.Update;          !!!! never use it !!!! -> methods doesnt work 
using Update = Telegram.Bot.Types.Update; 

class Program
{
    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;
    
    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;
    
    public static List<User> spotifyUsers = new ();       
    public static List<User> payedUsers = new ();
    
    private static readonly HttpClient client = new HttpClient();

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
            ThrowPendingUpdates = false  // for fix it use 19 version of library 
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
                            $"Всем привет это бот для ежемесячного \n {chat.Id} {_botClient}" +
                            $"напоминания об оплате подписки Spotify.\n" +
                            $"Каждый месяц 3 числа я буду отпарвлять уведомления в группу" +
                            $"о необходимости оплаты."
                            );
                        PaymentReminder();
                        return;
                    }
                    if (message.Text == "/payed")
                    {
                        var user = message.From;
                        long userId = user.Id;
                        if (CheckingUserMembership(userId) && UserNeedToPay(userId))
                        {
                            payedUsers.Add(RegisterNewUser(userId));
                            SaveUsers("C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\PayedUsers.json", payedUsers);
                            Console.WriteLine($"{user.Id} добавлен в список оплаченных");
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                $"пользователь {user.Username} оплатил!"
                            );
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                $"пользователь {user.Username} уже оплатил!"
                            );
                        }
                        return;
                    }

                    if (message.Text == "/registerNewUser")
                    {
                        var newUser = message.From;
                        long userId = newUser.Id;
                        if (spotifyUsers.Count < 5)
                        {
                            if (newUser != null && NeedToRegister(userId))
                            {
                                spotifyUsers.Add(RegisterNewUser(userId));
                                SaveUsers("C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\Subscribers.json", spotifyUsers);
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Новый пользователь зарегестрирован"
                                );
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Данный пользователь уже зарегестрирован!"
                                );
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Невозможно зарегестрировать нового пользователя \n" +
                                "достигнуто максимальное количество участников подписки"
                            );
                        }
                    }
                    if (message.Text == "/ddd")
                    {
                        SendDirectMessage();
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
            .UsingJobData("botClient", "6919816985:AAH3l0FCjEMtojvl4HRydn6ia0U6jPo51xc") // Pass the bot token as a string
            .Build();

        // Create a trigger to run the job every month on the 15th at 18:14
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("monthlyTrigger")
            .StartNow()
            .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(16, 21, 25))
            .Build();

        // Schedule the job
        await scheduler.ScheduleJob(job, trigger);

        Console.WriteLine("Press [Enter] to exit...");
    }

    private static bool CheckingUserMembership(long userId)
    {
        spotifyUsers =
            LoadUsers("C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\Subscribers.json");
        foreach (var user in spotifyUsers)
        {
            if (userId == user.Id)
            { 
                return true;
            }
        }
        return false;
    }

    private static bool UserNeedToPay(long userId)
    {
        payedUsers = LoadUsers("C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\PayedUsers.json");
        foreach (var user in payedUsers)
        {
            if (userId == user.Id)
            {
                return false;
            }
        }
        return true;
    }
    
    private static bool NeedToRegister(long userId)
    {
        spotifyUsers =
            LoadUsers("C:\\Users\\wonde\\Documents\\CSharp\\TelegtamBot\\SpotifyTelegramBot\\Subscribers.json");
        foreach (var user in spotifyUsers)
        {
            if (userId== user.Id)
            {
                return false;
            }
        }
        return true;
    }

    private static async void SendDirectMessage()
    {
        string botToken = "6919816985:AAH3l0FCjEMtojvl4HRydn6ia0U6jPo51xc"; // Replace with your bot token
        string userId = "1388592896 "; // Replace with the user's Telegram ID (same as chat ID for direct messages)
        string message = "Hello! This is a direct message from the bot.";

        string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={userId}&text={message}";

        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            System.Console.WriteLine("Message sent successfully!");
        }
        else
        {
            System.Console.WriteLine($"Failed to send message. Status code: {response.StatusCode}");
        }
    }
    
    private static User RegisterNewUser(long userId)
    {
        User newRegisterUser = new User();
        newRegisterUser.Id = userId;
        return newRegisterUser; 
    }
    
    static void SaveUsers(string filePath, List<User> users)
    {
        // Serialize the list to JSON and write it to the file
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    
    static List<User> LoadUsers(string filePath)
    {
        if (!File.Exists(filePath))
        {
            // If the file doesn't exist, return an empty list
            return new List<User>();
        }

        // Read and deserialize the JSON file
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }
}