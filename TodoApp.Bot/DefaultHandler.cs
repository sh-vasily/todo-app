using Telegram.Bot;
using Telegram.Bot.Types;

namespace TodoApp.Bot;

internal sealed class DefaultHandler : ICommandHandler
{
    private readonly ITelegramBotClient _telegramBotClient;

    public DefaultHandler(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    public async Task Handle(Update update, CancellationToken cancellationToken)
    {
        await _telegramBotClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Эта команда пока не поддерживается:\n" + update.Message.Text,
                    cancellationToken: cancellationToken);
    }
}