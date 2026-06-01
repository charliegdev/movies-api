using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository(IDbConnectionFactory dbConnectionFactory) : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                    insert into movies (id, slug, title, yearofrelease)
                    values (@Id, @Slug, @Title, @YearOfRelease)
                """,
                movie,
                cancellationToken: token
            )
        );

        bool insertSuccessful = result > 0;

        if (insertSuccessful)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        insert into genres (movieId, name)
                        values (@MovieId, @Name)
                        """,
                        new { MovieId = movie.Id, Name = genre },
                        cancellationToken: token
                    )
                );
            }
        }
        transaction.Commit();

        return insertSuccessful;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                select * from movies where id = @id
                """,
                new { id },
                cancellationToken: token
            )
        );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                select name from genres where movieid = @id
                """,
                new { id },
                cancellationToken: token
            )
        );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition(
                """
                select * from movies where slug = @slug
                """,
                new { slug },
                cancellationToken: token
            )
        );

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition(
                """
                select name from genres where movieid = @id
                """,
                new { id = movie.Id },
                cancellationToken: token
            )
        );

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        var result = await connection.QueryAsync(
            new CommandDefinition(
                """
                select m.*, string_agg(g.name, ',') as genres
                from movies m left join genres g on m.id = g.movieid
                group by id
                """,
                cancellationToken: token
            )
        );

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(',')),
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        // * Don't bother comparing old genres with new one. Just delete the old one and add the new one.
        // * Delete the old one
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                delete from genres where movieid = @id
                """,
                new { id = movie.Id },
                cancellationToken: token
            )
        );

        // * Add the new one
        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    insert into genres (movieid, name)
                    values (@MovieId, @Name)
                    """,
                    new { MovieId = movie.Id, Name = genre },
                    cancellationToken: token
                )
            );
        }

        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                update movies set slug = @slug, title = @Title, yearofrelease = @YearOfRelease
                where id = @Id
                """,
                movie,
                cancellationToken: token
            )
        );

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        using var transaction = connection.BeginTransaction();

        // * Remove the movie's genres first
        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                delete from genres where movieid = @id
                """,
                new { id },
                cancellationToken: token
            )
        );

        // * Then remove the movie itself
        var result = await connection.ExecuteAsync(
            new CommandDefinition(
                """
                delete from movies where id = @id
                """,
                new { id },
                cancellationToken: token
            )
        );

        transaction.Commit();

        return result > 0;
    }

    public async Task<bool> ExistsById(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                select count(1) from movies where id = @id
                """,
                new { id },
                cancellationToken: token
            )
        );
    }
}
