using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpotifyTelegramBot.Services;

public class BotService
{
    private static ITelegramBotClient _botClient;
    private static Update _update;
    private static  ReminderService _reminderService;

    public BotService(ITelegramBotClient botClient, Update update, ReminderService reminderService)
    {
        _botClient = botClient;
        _update = update;
        _reminderService = reminderService;
    }

    public void Command()
    {
        var message = _update.Message;
        var chat = message.Chat;

        switch (_update.Type)
        {
            case UpdateType.Message:
            {
                if (message.Text == "/start")
                {
                    SendWelcomMessage(chat.Id);
                    return;
                }
                
                if (message.Text == "/setReminder")
                {
                    RemindSender(chat.Id);
                    return;
                }

                return;
            }
        }
    }
    
    private static async Task SendWelcomMessage(long chatId)
    {
        await _botClient.SendTextMessageAsync(
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
    
    private static async Task RemindSender(long chatId)
    {
        await _botClient.SendTextMessageAsync(
            chatId,  
            $"Ежемесячные уведомления включены"
        );
        // PaymentReminder();
        _reminderService.PaymentReminder();
    }
    
}