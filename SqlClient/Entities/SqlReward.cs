using System.Data;
using DevRating.Domain;
using Microsoft.Data.SqlClient;

namespace DevRating.SqlClient.Entities
{
    internal sealed class SqlReward : Reward, IdentifiableObject
    {
        private readonly IDbConnection _connection;
        private readonly int _id;

        public SqlReward(IDbConnection connection, int id)
        {
            _connection = connection;
            _id = id;
        }

        public int Id()
        {
            return _id;
        }

        public Rating Rating()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT [RatingId] FROM [dbo].[Reward] WHERE [Id] = @Id";

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) {Value = _id});

            using var reader = command.ExecuteReader();

            reader.Read();

            return new SqlRating(_connection, (int) reader["RatingId"]);
        }

        public Author Author()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT [AuthorId] FROM [dbo].[Reward] WHERE [Id] = @Id";

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) {Value = _id});

            using var reader = command.ExecuteReader();

            reader.Read();

            return new SqlAuthor(_connection, (int) reader["AuthorId"]);
        }

        public double Value()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT [Reward] FROM [dbo].[Reward] WHERE [Id] = @Id";

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) {Value = _id});

            using var reader = command.ExecuteReader();

            reader.Read();

            return (double) reader["Reward"];
        }
    }
}