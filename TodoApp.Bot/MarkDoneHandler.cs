using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TodoApp.Contracts;

namespace TodoApp.Bot;

internal sealed class MarkDoneHandler : ICommandHandler
{
    private readonly HttpClient _httpClient;
    private readonly ITelegramBotClient _telegramBotClient;

    public MarkDoneHandler(HttpClient httpClient, ITelegramBotClient telegramBotClient)
    {
        _httpClient = httpClient;
        _telegramBotClient = telegramBotClient;
    }

    public async Task Handle(Update update, CancellationToken cancellationToken)
    {
        var (_, todoId) = update.Message.Text.Split(" ");

        var responseMessage = await _httpClient.PutAsync(
            $"http://localhost:80/api/todo/{todoId}", 
            default,
            cancellationToken);

        var todo = await responseMessage.Content.ReadFromJsonAsync<TodoViewModel>(
            cancellationToken: cancellationToken);
        
        await _telegramBotClient.SendTextMessageAsync(
            update.Message.Chat.Id,
            $"Задача {todo.Description} отмечена как сделанная",
            cancellationToken: cancellationToken);
    }
}