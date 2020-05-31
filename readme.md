# Entity Framework Core with GraphQL and SQL Server using HotChocolate

This article will cover two graphql options available for dotnet core:

- [HotChocolate](https://hotchocolate.io/)
- [GraphQL-DotNet](https://graphql-dotnet.github.io/)

<h2 id="create_project">Create the project</h2>

### Prerequisites

- VSCODE (Omnisharp + C# extensions installed)
- SQL Server
- .NET Core 3.1 SDK

First, we load up our trusty terminal (CMD, Bash, ZSH, PowerShell etc.) create a project folder and navigate to it and create the project:

```bash
> md netcore-graphql-test
> cd netcore-graphql-test
> dotnet new web
```

This wil create a new **empty** web application. Once that's done, we install the dependencies:

```bash
> dotnet add package Microsoft.EntityFrameworkCore
> dotnet add package Microsoft.EntityFrameworkCore.SqlServer
> dotnet add package HotChocolate.AspNetCore
> dotnet add package HotChocolate.AspNetCore.Playground
> code .
```

This will install the above packages, and open up vscode. When you open up vscode, you may see some popups, just click install for C# extensions, wait for Omnisharp to install if it already isn't installed, and let it install any missing required assets.

So far, our csproj file should look like this:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <RootNamespace>netcore_graphql_test</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="HotChocolate.AspNetCore" Version="10.4.3" />
    <PackageReference Include="HotChocolate.AspNetCore.Playground" Version="10.4.3" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="3.1.4" />
  </ItemGroup>

</Project>
```

## Startup.cs

```csharp
using HotChocolate;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace netcore_graphql_test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQL(SchemaBuilder.New()
                // AddQueryType<T>() here 
                .Create());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseWebSockets()
                .UseGraphQL()
                .UsePlayground();
        }
    }
}
```

This boilerplate, will allow us to eventually use a GraphiQL style playground to test our queries.

## The Database
In our test database for this example, lets say we have a table

```sql
CREATE TABLE [Locations] (
  [ID] [int] IDENTITY(1,1) NOT NULL,
  [Name] [nvarchar](50) NOT NULL,
  [Code] [nvarchar](5) NOT NULL,
  [Active] [bit] NOT NULL
)
```

Lets insert some test data:

```sql
INSERT INTO [Locations]
  ([Name], [Code], [Active])
VALUES
  ('Sydney', 'SYD', 1)
GO
INSERT INTO [Locations]
  ([Name], [Code], [Active])
VALUES
  ('Los Angeles', 'LAX', 1)
GO
```

## Entity
This is pretty much the same as your standard .NET EntityFramework entity you'd use. It's basically a class with the matching column names and types as the `CREATE TABLE` query above.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace netcore_graphql_test
{
    [Table("Locations")]
    public class Location
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, MaxLength(5)]
        public string Code { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
```

## The DB Context

Again, it's pretty much identical to any standard DB Context in .NET Entity Framework Core.

```csharp
using Microsoft.EntityFrameworkCore;

namespace netcore_graphql_test
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
    }
}
```

## The Schema

Hear comes the meaty part. This is what we will have access to in our graphql playground. As a basic query, we'll return a list of Locations from our Location table, and another query that accepts an argument for a specific location code. The schema is pretty straightforward, you'll have one class that has queries, and another class that extends ObjectType and configures the fields for the query. The latter will be added to our Startup.cs file as a QueryType in the Schema definition.

First, we'll create the query class. This contains all the queries for this example.

```csharp
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netcore_graphql_test
{
    public class LocationQueries
    {
        /// <summary>
        /// Return a list of all locations
        /// Notice the [Service]. It's an auto look-up from HotChocolate
        /// </summary>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public async Task<List<Location>> GetLocations([Service] MyDbContext dbContext) =>
            await dbContext.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync();

        
        public async Task<List<Location>> GetLocation([Service] MyDbContext dbContext, string code) =>
            await dbContext.Locations
            .AsNoTracking()
            .Where(l => l.Code == code)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }
}
```

Next, we create the query type.

```csharp
using HotChocolate.Types;

namespace netcore_graphql_test
{
    public class LocationQueryType : ObjectType<LocationQueries>
    {
        protected override void Configure(IObjectTypeDescriptor<LocationQueries> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Field(x => x.GetLocations(default));

            descriptor.Field(x => x.GetLocation(default, default))
                .Argument("code", a => a.Type<StringType>());
        }
    }
}
```

This lets our schema know that we have 2 queries available. `GetLocations` will get all locations. `GetLocation` will get a location by `code`.

## Back to Startup.cs file

Now that we have everything set up, we can update our Startup.cs file with the data context, and schema builder.

To do this, the update the `ConfigureServices` function with the following:

```csharp
public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add DbContext
            services.AddDbContext<MyDbContext>(options =>
            {                
                options.UseSqlServer(Configuration.GetConnectionString("Default"));
            });

            // Add GraphQL Services
            services
                .AddDataLoaderRegistry()
                .AddGraphQL(SchemaBuilder.New()
                // LocationQueryType as a QueryType
                .AddQueryType<LocationQueryType>().Create());
        }
```

## Build and run

To build and run, type the following in your terminal:

```bash
> dotnet run
```

Open your browser to http://localhost:5000/playground. This will display a `graphiql` playground. In this you should be able to successfully run the queries:

```json
# return all locations

{
  locations {
    name
    code
  }
}

# return "Los angeles"

{
  location(code: "lax") {
    name
  }
}
```

This covers the basic fundamentals of EFCore and GraphQL. Hope it helps someone.

