using Newtonsoft.Json;

namespace SpotifyTelegramBot.Maping;

public class UserMaping
{
    private static string  filePathSubscribers = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/Subscribers.json"));
    private static string  filePathPayedUser = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),"..", "..", "..","json/PayedUsers.json"));
    
    private static List<User> spotifyUsers = new ();
    private static List<User> payedUsers = new ();

    public static bool CheckingUserMembership(long userId) 
    {
        spotifyUsers =
            LoadUsersSubscribers();
        foreach (var user in spotifyUsers)
        {
            if (userId == user.Id)
            { 
                return true;
            }
        }
        return false;
    }

    public static bool UserNeedToPay(long userId) 
    {
        payedUsers = LoadUsersPayed();
        foreach (var user in payedUsers)
        {
            if (userId == user.Id)
            {
                return false;
            }
        }
        return true;
    }
    
    public static bool NeedToRegister(long userId) 
    {
        spotifyUsers =
            LoadUsersSubscribers(); 
        foreach (var user in spotifyUsers)
        {
            if (userId== user.Id)
            {
                return false;
            }
        }
        return true;
    }
    
    
    public static User RegisterNewUser(long userId) 
    {
        User newRegisterUser = new User();
        newRegisterUser.Id = userId;
        return newRegisterUser; 
    }
    
    public static void SaveUsers(List<User> users) 
    {
        string json = JsonConvert.SerializeObject(users, Formatting.Indented);
        File.WriteAllText(filePathSubscribers, json);
    }
    
    public static List<User> LoadUsersSubscribers() 
    {
        if (!File.Exists(filePathSubscribers))
        {
            return new List<User>();
        }
        string json = File.ReadAllText(filePathSubscribers);
        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }
    
    public static List<User> LoadUsersPayed() 
    {
        if (!File.Exists(filePathPayedUser))
        {
            return new List<User>();
        }
        string json = File.ReadAllText(filePathPayedUser);
        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }
    
    public List<User> LoadUsers(string filePath) 
    {
        if (!File.Exists(filePath))
        {
            return new List<User>();
        }
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
    }

}