# GraphQL and ASP.NET Core Minimal Hosting

This demo uses Entity Framework Core (In-Memory), [Hot Chocolate](https://github.com/ChilliCream/hotchocolate), and Bogus to create a 
GraphQL demo that exposes your DbContext over an API.

It gives you the following:

- Projections (i.e. `select id from table`)
- Sorting (`asc` `desc`)
- Filtering (where clauses)
- Pagination (first 5 of query)

## Getting Started

This is a self contained project, so restore the packages and run the project. I'm targeting `.NET 6`, so anything higher should work.

```console
dotnet restore && dotnet run
```

## GraphQL Query To Start

When the app starts, it will redirect you to **Banana Cake Pop** the GraphQL query interface for Hot Chocolate.
Try playing around with the query below. It retrieves people and the building they are in.

```graphql
{
  people(
      order: { id: ASC }, 
      where: { building : { id : { gt : 500 } } },
      first: 3
    ) 
    {
    totalCount,
    pageInfo {
      hasNextPage,
      hasPreviousPage,
      startCursor,
      endCursor
    },
    nodes {
      id,
      fullName,
      building {
        id,
        name
      }
    }    
  }
}
```

## Notes

- `DbContextFactory` is necessary to allow for future parallel queries.
- `IQueryable` is important but not necessary
- Attribute ordering on the [`Query`](./RidingTheGraph/Program.cs#L79) is really important.
- Changes to attributes can change the entire schema, so it's best to always refresh and check the schema if your query errors out.
- Projections are important when working with Entity Framework, as you won't be able to load the related data without it, or you'd have to enable Lazy Loading (don't enable lazy loading).