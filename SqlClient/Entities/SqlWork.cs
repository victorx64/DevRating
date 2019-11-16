using System.Collections.Generic;
using System.Data;
using DevRating.Domain;
using Microsoft.Data.SqlClient;

namespace DevRating.SqlClient.Entities
{
    internal sealed class SqlWork : IdentifiableWork
    {
        private readonly IDbConnection _connection;
        private readonly int _id;

        public SqlWork(IDbConnection connection, int id)
        {
            _connection = connection;
            _id = id;
        }

        public int Id()
        {
            return _id;
        }

        public double Reward()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT Reward FROM Work WHERE Id = @Id";

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) {Value = _id});

            using var reader = command.ExecuteReader();

            reader.Read();

            return (float) reader["Reward"];
        }

        public string Author()
        {
            using var command = _connection.CreateCommand();

            command.CommandText = @"
            SELECT Email
            FROM Work
            INNER JOIN Author ON Work.AuthorId = Author.Id 
            WHERE Work.Id = @Id";

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) {Value = _id});

            using var reader = command.ExecuteReader();

            reader.Read();

            return (string) reader["Email"];
        }

        public IEnumerable<RatingUpdate> RatingUpdates()
        {
            throw new System.NotImplementedException();
        }
    }
}