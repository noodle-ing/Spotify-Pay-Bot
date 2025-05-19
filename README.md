# Spotify Pay Bot

**Spotify Pay Bot** is a Telegram bot built with C# and .NET that notifies Spotify subscription members when their subscription payment is due. It simplifies subscription management by sending timely reminders via Telegram.

## 📝 Features

* **Automated Reminders**: Sends reminders to subscribers before their Spotify payment is due.
* **Subscriber Management**: Add, view, and remove subscribers easily.
* **Customizable Schedule**: Configure reminder timings according to your needs.
* **Data Persistence**: Stores subscriber information in a JSON file.
* **Docker Support**: Run the bot in a containerized environment for easy deployment.

## 🚀 Architecture

* **Controllers**: Handle Telegram bot commands and interactions.
* **Services**: Contain business logic for scheduling and notifications.
* **Models**: Define data structures such as subscribers and remider messages.
* **Mapping**: Automaps models and view models for clean separation.
* **Reminders**: Scheduler functionality to check due payments and send messages.

## ⚙️ Getting Started

### Prerequisites

* [.NET 6.0 SDK](https://dotnet.microsoft.com/) or later
* [Docker](https://www.docker.com/) (optional, for container deployment)
* A Telegram bot token (create one via [@BotFather](https://t.me/BotFather))

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/noodle-ing/Spotify-Pay-Bot.git
   cd Spotify-Pay-Bot
   ```

2. **Configure environment variables**

   Create a `.env` file in the project root with the following content:

   ```ini
   TELEGRAM_BOT_TOKEN=your_telegram_bot_token
   SUBSCRIBERS_FILE=subscribers.json
   REMINDER_CRON="0 9 * * *"   # Send reminders daily at 09:00 AM
   ```

3. **Run the application**

   ```bash
   dotnet run --project SpotifyTelegramBot.csproj
   ```

4. **Add subscribers**

   * Send `/add subscriber_username:yyyy-MM-dd` to the bot to add a subscriber with their next payment date.
   * Example: `/add alice:2025-06-01`

5. **List subscribers**

   * Send `/list` to view all subscribers and their payment dates.

6. **Remove subscribers**

   * Send `/remove subscriber_username` to remove a subscriber.

## 🐳 Docker Deployment

1. **Build the Docker image**

   ```bash
   docker build -t spotify-pay-bot .
   ```

2. **Run the container**

   ```bash
   docker run --env-file .env spotify-pay-bot
   ```
