using System.Net.Http.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using TodoApp.Contracts;

namespace TodoApp.Bot;

internal sealed class GetAllTodoHandler : ICommandHandler
{
    private readonly HttpClient _httpClient;
    private readonly ITelegramBotClient _telegramBotClient;

    internal GetAllTodoHandler(HttpClient httpClient, ITelegramBotClient telegramBotClient)
    {
        _httpClient = httpClient;
        _telegramBotClient = telegramBotClient;
    }

    public async Task Handle(Update update, CancellationToken cancellationToken)
    {
        var args = update.Message.Text.Split(' ', 2);

        var todos = await _httpClient.GetFromJsonAsync<List<TodoViewModel>>("http://localhost:80/api/todo",
            cancellationToken: cancellationToken);
        
        if (args.Length == 2)
        {
            var filter = args[1];
            todos = filter switch
            {
                "/done" => todos.Where(todo => todo.Done).ToList(),
                "/notdone" => todos.Where(todo => !todo.Done).ToList(),
                _ => todos
            };
        }

        var responseText = todos
            .Select(todo => $"{todo.Id}\n{todo.Description}\n{todo.Comment}\n{(todo.Done ? "Сделано" : "Не сделано")}\n\n")
            .Aggregate((x, y) => $"{x}\n {y}");


        await _telegramBotClient.SendTextMessageAsync(
            update.Message.Chat.Id,
            responseText,
            cancellationToken: cancellationToken);
    }
}