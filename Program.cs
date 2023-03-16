using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TarefaDB"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/tarefas", async (AppDbContext db) => await db.Tarefas.ToListAsync());

app.MapGet("/tarefas/{id:int}", async (AppDbContext db, int id) =>
    await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.BadRequest());

app.MapGet("/tarefas/concluida", async (AppDbContext db) =>
    await db.Tarefas.Where(p => p.IsConcluida).ToListAsync());

app.MapPost("/tarefas", async (AppDbContext db, Tarefa tarefa) =>
{
    db.Tarefas.Add(tarefa);
    db.SaveChanges();

    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (int id, Tarefa tarefa, AppDbContext db) =>
{
    var tarefaEncontrada = await db.Tarefas.FindAsync(id);

    if (tarefaEncontrada is null) return Results.NotFound();

    tarefaEncontrada.Name = tarefa.Name;
    tarefaEncontrada.IsConcluida = tarefa.IsConcluida;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});

app.Run();


class Tarefa
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}