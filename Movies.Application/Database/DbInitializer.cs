using Dapper;

namespace Movies.Application.Database;

public class DbInitializer(IDbConnectionFactory dbConnectionFactory)
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        // Create a new movie database if not exist
        await connection.ExecuteAsync(
            """
                create table if not exists movies (
                id UUID primary key,
                slug TEXT not null,
                title TEXT not null,
                yearofrelease integer not null);
            """
        );

        // Give slug an index as well
        await connection.ExecuteAsync(
            """
            create unique index concurrently if not exists movies_slug_idx
            on movies
            using btree(slug);
            """
        );

        // Create a table for genres as well
        await connection.ExecuteAsync(
            """
            create table if not exists genres (
            movieid UUID references movies (Id),
            name TEXT not null);
            """
        );

        await connection.ExecuteAsync(
            """
            create table if not exists ratings (
            userid uuid,
            movieid uuid references movies(id),
            rating integer not null,
            primary key (userid, movieid)
            );
            """
        );
    }
}
