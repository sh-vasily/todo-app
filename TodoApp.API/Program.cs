using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using MongoDB.Bson;
using MongoDB.Driver;

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/todo", async (CancellationToken cancellationToken) =>
{
    var todos = await todoCollection
        .Find(_ => true)
        .ToListAsync(cancellationToken);
    
    return todos.Select(TodoViewModel.FromTodo);
});

app.MapPut("/api/todo/{todoId}",
    async (string todoId, CancellationToken cancellationToken) => 
        await todoCollection.FindOneAndUpdateAsync(
                todo => todo.Id == ObjectId.Parse(todoId), 
                Builders<Todo>.Update.Set(todo => todo.Done, true), 
                cancellationToken: cancellationToken));

app.MapPost("/api/todo",
    async (CreateTodoViewModel todoViewModel, CancellationToken cancellationToken) => 
        await todoCollection.InsertOneAsync(
            Todo.FromDescription(todoViewModel.Description), 
            cancellationToken: cancellationToken));

app.Run();

internal record Todo(ObjectId Id, string Description, bool Done)
{
    public static Todo FromDescription(string description)
        => new (ObjectId.GenerateNewId(), description, false);
}

internal record TodoViewModel(string Id, string Description, bool Done)
{
    public static TodoViewModel FromTodo(Todo todo)
        => new (todo.Id.ToString(), todo.Description, todo.Done);
};

record CreateTodoViewModel(string Description); 