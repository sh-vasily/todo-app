using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TodoApp.Bot;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var token = config.GetSection("token").Value;

var botClient = new TelegramBotClient(token);

using CancellationTokenSource cts = new();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};

using HttpClient client = new();
client.DefaultRequestHeaders.Accept.Clear();
client.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

var commandHandlersFactory = new CommandHandlersFactory(botClient, client);

var handlers = 
    new List<string> { Commands.MarkDone, Commands.AllTodos, Commands.AddTodo }
    .ToDictionary(cmd => cmd, cmd => commandHandlersFactory.CreateInstance(cmd));

var defaultHandler = commandHandlersFactory.CreateInstance();

botClient.StartReceiving(
    HandleUpdateAsync,
    HandlePollingErrorAsync,
    receiverOptions,
    cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message

    if (update.Message is not { Text: { } messageText } message)
        return;

    var command = update.Message?.Entities?.First(entity => entity.Type is MessageEntityType.BotCommand);

    if (command is null)
        return;

    var chatId = message.Chat.Id;
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

    var commandText = update.Message?.Text.ToLower().Split(" ").First();
    var handler = handlers.GetValueOrDefault(commandText, defaultHandler);
    await handler.Handle(update, cancellationToken);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var errorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(errorMessage);
    return Task.CompletedTask;
}