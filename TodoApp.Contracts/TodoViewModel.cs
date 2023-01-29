namespace TodoApp.Contracts;

public record TodoViewModel(string Id, string Description, bool Done, string? Comment = null);
public record CreateTodoViewModel(string Description, string Comment);