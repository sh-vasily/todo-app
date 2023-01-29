using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TodoApp.Contracts;

namespace TodoApp.Bot;

internal sealed class AddTodoHandler : ICommandHandler
{
    private readonly HttpClient _httpClient;
    private readonly ITelegramBotClient _telegramBotClient;

    public AddTodoHandler(HttpClient httpClient, ITelegramBotClient telegramBotClient)
    {
        _httpClient = httpClient;
        _telegramBotClient = telegramBotClient;
    }

    public async Task Handle(Update update, CancellationToken cancellationToken)
    {
        var (_, todoDescription) = update.Message.Text.Split(' ', 2);

        await _httpClient.PostAsJsonAsync(
            "http://localhost:80/api/todo", 
            new CreateTodoViewModel(todoDescription, string.Empty),
            cancellationToken);

        await _telegramBotClient.SendTextMessageAsync(
            update.Message.Chat.Id,
            $"Задача '{todoDescription}' добавлена",
            cancellationToken: cancellationToken);
    }
}