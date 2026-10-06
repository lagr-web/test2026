using Microsoft.EntityFrameworkCore;

namespace test2026
{
    public class TodoDb: DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options):base(options)
        { }
        // Dette DbSet repræsenterer vores tabel "Todos" i SQLite-databasen
        public DbSet<Todo> Todos => Set<Todo>();

    }
}
