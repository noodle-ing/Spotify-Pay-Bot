using SpotifyTelegramBot.Maping;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SpotifyTelegramBot.Services;

public class UserService
{
    
    public void UserCommandHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var message = update.Message;
        var chat = message.Chat;
        switch (update.Type)
        {
            case UpdateType.Message:
            {
                if (message.Text == "/payed")
                {
                    var user = message.From;
                    long userId = user.Id;
                    var userName = user.Username;
                    PaymentMessage( userId, chat.Id, userName, botClient);
                    return;
                }

                if (message.Text == "/registerNewUser")
                {
                    var newUser = message.From;
                    long userId = newUser.Id;
                    NewUserRegistration(botClient, newUser.Username, userId, chat.Id, botClient);
                    return;

                }
                return;
            }
        }
    }
    
    private static async Task PaymentMessage(long userId, long chatId, string userName, ITelegramBotClient _botClient) 
    {
        List<User> payedUsers = UserMaping.LoadUsersPayed();
        if (UserMaping.CheckingUserMembership(userId))
        {
            if (UserMaping.UserNeedToPay(userId))
            {
                var newUser = UserMaping.RegisterNewUser(userId);
                payedUsers.Add(newUser);
                UserMaping.SaveUsers( payedUsers);
                Console.WriteLine($"{userId} добавлен в список оплаченных");
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"пользователь {userName} оплатил!"
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    $"пользователь {userName} уже оплатил"
                );
            }
            
        }
        else
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                $"пользователю {userName} сперва нужно зарегестрироватся"
            );
        }
    }
    
    
    private static async Task NewUserRegistration(ITelegramBotClient botClient,string userName, long userId, long chatId, ITelegramBotClient _botClient) 
    {
        List<User> spotifyUsers = UserMaping.LoadUsersSubscribers();
        if (spotifyUsers.Count < 5)
        {
            if (UserMaping.NeedToRegister(userId))
            {
                spotifyUsers.Add(UserMaping.RegisterNewUser(userId));
                UserMaping.SaveUsers(spotifyUsers);
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Новый пользователь зарегестрирован"
                );
            }
            else
            {
                await _botClient.SendTextMessageAsync(
                    chatId,
                    "Данный пользователь уже зарегестрирован!"
                );
            }
        }
        else
        {
            await _botClient.SendTextMessageAsync(
                chatId,
                "Невозможно зарегестрировать нового пользователя \n" +
                "достигнуто максимальное количество участников подписки"
            );
        }
    }

}