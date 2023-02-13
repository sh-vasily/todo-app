using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TodoApp.Contracts;

var projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;

var mongoContainer = new TestcontainersBuilder<MongoDbTestcontainer>()
    .WithImage("mongo")
    .WithPortBinding(27017)
    .WithBindMount($@"{projectDirectory}\data", "/data/db", AccessMode.ReadWrite)
    .Build();

var containerTask = mongoContainer.StartAsync();

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

app.MapGet("/api/tags", async (CancellationToken cancellationToken) =>
{
    await todoCollection
        .AsQueryable()
        .SelectMany(todo => todo.Tags)
        .Distinct()
        .ToListAsync(cancellationToken);
});

app.MapGet("/api/todo", async ([FromQuery]string[] tags, CancellationToken cancellationToken) =>
{
    var todos = await todoCollection
        .Find(Builders<Todo>.Filter.Where(todo => !tags.Any() || tags.Contains("Done", StringComparer.OrdinalIgnoreCase) && todo.Done))
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

app.MapDelete("/api/todo/{todoId}",
    async (string todoId, CancellationToken cancellationToken) =>
    {
        var todo = await todoCollection.DeleteOneAsync(
            Builders<Todo>.Filter.Where(todo => todo.Id == ObjectId.Parse(todoId)),
            cancellationToken);
    });

app.MapPost("/api/todo",
    async (CreateTodoViewModel todoViewModel, CancellationToken cancellationToken) => 
        await todoCollection.InsertOneAsync(
            Todo.FromViewModel(todoViewModel), 
            cancellationToken: cancellationToken));


var appTask = app.RunAsync();

await Task.WhenAll(containerTask, appTask);

internal record Todo(ObjectId Id,
    string Description,
    bool Done,
    string? Comment = null,
    string[]? Tags = null)
{
    public static Todo FromViewModel(CreateTodoViewModel createTodoViewModel)
        => new (ObjectId.GenerateNewId(), createTodoViewModel.Description, false, 
            createTodoViewModel.Comment, createTodoViewModel.Tags);
}

internal static class TodoExtensions
{
    public static TodoViewModel ToViewModel(this Todo todo)
        => new (todo.Id.ToString(), todo.Description, todo.Done, todo.Comment, todo.Tags);
}       