using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using MongoDB.Bson;
using MongoDB.Driver;
using TodoApp.Contracts;

var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;

var mongoContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
    .WithImage("mongo")
    .WithPortBinding(27017)
    .WithBindMount($@"{projectDirectory}\data", "/data/db", AccessMode.ReadWrite)
    .Build();

await mongoContainer.StartAsync();

var mongoClient = new MongoClient("mongodb://localhost:27017");
var todoCollection = mongoClient.GetDatabase("todo-api-db").GetCollection<Todo>("todo");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/todo", async (CancellationToken cancellationToken) =>
{
    var todos = await todoCollection
        .Find(_ => true)
        .ToListAsync(cancellationToken);
    
    return todos.Select(todo => todo.ToViewModel());
});

app.MapPut("/api/todo/{todoId}",
    async (string todoId, CancellationToken cancellationToken) =>
    {
        var todo = await todoCollection.FindOneAndUpdateAsync(
            Builders<Todo>.Filter.Where(todo => todo.Id == ObjectId.Parse(todoId)),
            Builders<Todo>.Update.Set(todo => todo.Done, true),
            new FindOneAndUpdateOptions<Todo>
            {
                ReturnDocument = ReturnDocument.After,
            },
            cancellationToken);
        
        return todo.ToViewModel();
    });

app.MapPost("/api/todo",
    async (CreateTodoViewModel todoViewModel, CancellationToken cancellationToken) => 
        await todoCollection.InsertOneAsync(
            Todo.FromViewModel(todoViewModel), 
            cancellationToken: cancellationToken));

app.Run();

internal record Todo(ObjectId Id, string Description, bool Done, string? Comment = null)
{
    public static Todo FromViewModel(CreateTodoViewModel createTodoViewModel)
        => new (ObjectId.GenerateNewId(), createTodoViewModel.Description, false, createTodoViewModel.Comment);
}

internal static class TodoExtensions
{
    public static TodoViewModel ToViewModel(this Todo todo)
        => new (todo.Id.ToString(), todo.Description, todo.Done, todo.Comment);
}       