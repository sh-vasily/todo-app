using Telegram.Bot;

namespace TodoApp.Bot;

internal sealed class CommandHandlersFactory
{
    private readonly HttpClient _httpClient;
    private readonly ITelegramBotClient _telegramBotClient;

    public CommandHandlersFactory(ITelegramBotClient telegramBotClient, HttpClient httpClient)
    {
        _telegramBotClient = telegramBotClient;
        _httpClient = httpClient;
    }

    internal ICommandHandler CreateInstance(string command = "")
        => command switch
        {
            Commands.MarkDone => new MarkDoneHandler(_httpClient, _telegramBotClient),
            Commands.AllTodos => new GetAllTodoHandler(_httpClient, _telegramBotClient),
            Commands.AddTodo => new AddTodoHandler(_httpClient, _telegramBotClient),
            _ => new DefaultHandler(_telegramBotClient),
        };
}