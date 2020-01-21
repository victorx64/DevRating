using System.Data;
using DevRating.DefaultObject;
using DevRating.Domain;
using Microsoft.Data.Sqlite;

namespace DevRating.SqliteClient
{
    internal sealed class SqliteRating : Rating
    {
        private readonly IDbConnection _connection;
        private readonly Id _id;

        public SqliteRating(IDbConnection connection, Id id)
        {
            _connection = connection;
            _id = id;
        }

        public Id Id()
        {
            return _id;
        }

        public string ToJson()
        {
            throw new System.NotImplementedException();
        }

        public Rating PreviousRating()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT PreviousRatingId FROM Rating WHERE Id = @Id";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            reader.Read();

            return new SqliteRating(_connection, new DefaultId(reader["PreviousRatingId"]));
        }

        public bool HasDeletions()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT Deletions FROM Rating WHERE Id = @Id AND Deletions IS NOT NULL";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            return reader.Read();
        }

        public uint Deletions()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT Deletions FROM Rating WHERE Id = @Id AND Deletions IS NOT NULL";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            reader.Read();

            return (uint) (long) reader["Deletions"];
        }

        public bool HasPreviousRating()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT PreviousRatingId FROM Rating WHERE Id = @Id AND PreviousRatingId IS NOT NULL";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            return reader.Read();
        }

        public Work Work()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT WorkId FROM Rating WHERE Id = @Id";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            reader.Read();

            return new SqliteWork(_connection, new DefaultId(reader["WorkId"]));
        }

        public Author Author()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT AuthorId FROM Rating WHERE Id = @Id";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            reader.Read();

            return new SqliteAuthor(_connection, new DefaultId(reader["AuthorId"]));
        }

        public double Value()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT Rating FROM Rating WHERE Id = @Id";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = _id.Value()});

            using var reader = command.ExecuteReader();

            reader.Read();

            return (double) reader["Rating"];
        }
    }
}