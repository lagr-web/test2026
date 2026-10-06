
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using test2026.Models.library;

namespace test2026
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // TILFØJET: Fikser "Object cycle detected" fejlen for Books/Genres relationen med det samme
            
            builder.Services.ConfigureHttpJsonOptions(options => {
                options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });
            

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("NextJsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:3000") // Next.js standard adresse
                          .AllowAnyMethod()                    // Tillad GET, POST, PUT, DELETE
                          .AllowAnyHeader();                   // Tillad alle headers (f.eks. JSON-kald)
                });
            });


            // Add services to the container.
            //builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            
            builder.Services.AddDbContext<TodoDb>(options =>
            options.UseSqlite("Data Source=Todo.db"));
            

            builder.Services.AddDbContext<BooksContext>();//vigtigt til vores books...

            var app = builder.Build();

            // 2. AKTIVER CORS (Skal stå efter app.Build(), men FØR dine endpoints)
            app.UseCors("NextJsPolicy");


            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();


                app.UseSwaggerUI(options =>
                {
                    // Vi beder Swagger om at læse den samme .NET 10 JSON-fil
                    options.SwaggerEndpoint("/openapi/v1.json", "Mit API v1");
                });
            }

            app.UseHttpsRedirection();

            //app.UseAuthorization();


            app.MapGet("/", () => "Hello World");

            app.MapGet("/getData", async (TodoDb db) =>
            {
                var todoItems = await db.Todos.ToListAsync();

                return Results.Ok(todoItems);


            });

            app.MapPost("/createData", async (Todo todo, TodoDb db) => {

                db.Todos.Add(todo);
                await db.SaveChangesAsync();
                return Results.Created($"/createData/{todo.Id}", todo);

            });

            app.MapGet("/getData/{id}", async (int id, TodoDb db) =>
            {
                var todoItem = await db.Todos.FindAsync(id);

                if (todoItem is null)
                {
                    return Results.NotFound($"Todo-punkt med ID {id} blev ikke fundet.");
                }

                return Results.Ok(todoItem);
            });

            app.MapDelete("/getData/{id}", async (int id, TodoDb db) => {

                if (await db.Todos.FindAsync(id) is Todo todo)
                {
                    db.Todos.Remove(todo);
                    await db.SaveChangesAsync();
                    return Results.Ok($"Todo-post med ID {id} er slettet");

                }

                return Results.NotFound();
            
            });

            app.MapPut("/getData/{id}", async(int id, Todo inputTodo, TodoDb db) => {

                var todo = await db.Todos.FindAsync(id);

                if (todo is null) return Results.NotFound();

                todo.Name = inputTodo.Name;
                todo.IsComplete = inputTodo.IsComplete;
                todo.Comment = inputTodo.Comment;

                await db.SaveChangesAsync();

                return Results.Ok($"Todo-post med ID {id} er opdateret.");
            
            });




            //***** books **************************************************************//

            //Du kan oprettet en model og en DbContext med følgende:

            // dotnet ef dbcontext scaffold "Data Source=books.db" Microsoft.EntityFrameworkCore.Sqlite -o Models

            //DbContext er:
            /*
             Man kan godt kalde det en slags "bro" eller "formidler" 
             mellem din C#-kode og databasen, men det tekniske udtryk inden for Entity Framework Core er en Context-klasse.
             
             * */

            app.MapGet("/getGenres", async (BooksContext db) =>
            {
                // Henter KUN genrerne 
                var genres = await db.Genres.ToListAsync();
                return Results.Ok(genres);
            });

            // Finder alle bøger, hvor fremmednøglen matcher det ID, der klikkes på
            app.MapGet("/getBooksByGenre/{genreId}", async (int genreId, BooksContext db) =>
            {
                var books = await db.Books
                    .Where(b => b.GenreId == genreId)
                    .ToListAsync();

                return Results.Ok(books);
            });


            // denne henter das alles

            app.MapGet("/getBooks", async (BooksContext db) =>
            {

                var books = await db.Books.Include(b => b.Genre).ToListAsync();
                return Results.Ok(books);

            });
            
            

            app.Run();
        }
    }
}
