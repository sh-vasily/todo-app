using Telegram.Bot.Types;

namespace TodoApp.Bot;

internal interface ICommandHandler
{
    Task Handle(Update update, CancellationToken cancellationToken);
}