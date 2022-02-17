using System.Diagnostics;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPooledDbContextFactory<Database>(b =>
    b.UseInMemoryDatabase("Test")
        .EnableSensitiveDataLogging()
        .LogTo(message => Debug.WriteLine(message))
);

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddQueryableCursorPagingProvider();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
    await Database.Initialize(scope);

// redirect to Banana Cake Pop
app.MapGet("/", () => Results.LocalRedirect("/graphql"));
app.MapGraphQL();
app.Run();

public class Database : DbContext
{
    public Database(DbContextOptions<Database> options)
        : base(options) {}

    public DbSet<Person>? People { get; set; }
    
    public static async Task Initialize(AsyncServiceScope scope)
    {
        var people = new Bogus.Faker<Person>()
            .RuleFor(x => x.Id, x => ++x.IndexVariable)
            .RuleFor(x => x.FullName, x => x.Name.FullName())
            .Generate(1000);

        var buildings = new Bogus.Faker<Building>()
            .RuleFor(x => x.Id, x => ++x.IndexVariable)
            .RuleFor(x => x.Name, x => x.Address.FullAddress())
            .Generate(1000);

        foreach (var person in people)
        {
            var random = RandomNumberGenerator.GetInt32(0, buildings.Count);
            person.Building = buildings[random];
        }

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<Database>>();
        await using var database = await factory.CreateDbContextAsync();

        //await database.Buildings!.AddRangeAsync(buildings);
        await database.People!.AddRangeAsync(people);
        await database.SaveChangesAsync();
    }
}

public class Person
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public Building Building { get; set; } = default!;
}

public class Building
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class Query
{
    [UseDbContext(typeof(Database)), 
     UsePaging(MaxPageSize = 100, IncludeTotalCount = true), 
     UseProjection,
     UseFiltering,
     UseSorting]
    public IQueryable<Person>? GetPeople([ScopedService] Database db)
        => db.People;
}