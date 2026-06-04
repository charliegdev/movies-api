using System.Data;
using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository(IDbConnectionFactory dbConnectionFactory) : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<float?>(
            new CommandDefinition(
                """
                select round(avg(r.rating), 1) from ratings r
                where movieid = @movieId
                """,
                new { movieId },
                cancellationToken: token
            )
        );
    }

    public async Task<(float? Rating, int? UserRating)> GetRatingAsync(
        Guid movieId,
        Guid userId,
        CancellationToken token = default
    )
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(
            new CommandDefinition(
                """
                select 
                    round(avg(rating), 1) as rating,
                    (select rating from ratings where movieid = @movieId and userid = @userId limit 1) as userrating
                from ratings
                where movieid = @movieId
                """,
                new { movieId, userId },
                cancellationToken: token
            )
        );
    }

    public async Task<bool> RateMovieAsync(
        Guid movieId,
        int rating,
        Guid userId,
        CancellationToken token
    )
    {
        using var connnection = await _dbConnectionFactory.CreateConnectionAsync(token);
        var result = await connnection.ExecuteAsync(
            new CommandDefinition(
                """
                insert into ratings(userid, movieid, rating)
                values (@userId, @movieId, @rating)
                on conflict (userid, movieid) do update
                    set rating = @rating
                """,
                new
                {
                    userId,
                    movieId,
                    rating,
                },
                cancellationToken: token
            )
        );

        return result > 0;
    }
}
