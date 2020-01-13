using System.Data;
using DevRating.Domain;
using Microsoft.Data.Sqlite;

namespace DevRating.SqliteClient
{
    internal sealed class SqliteWorks : Works
    {
        private readonly IDbConnection _connection;
        private readonly InsertWorkOperation _insert;
        private readonly GetWorkOperation _get;

        public SqliteWorks(IDbConnection connection) : this(
            connection,
            new SqliteInsertWorkOperation(connection),
            new SqliteGetWorkOperation(connection))
        {
        }

        public SqliteWorks(IDbConnection connection, InsertWorkOperation insert, GetWorkOperation get)
        {
            _connection = connection;
            _insert = insert;
            _get = get;
        }

        public InsertWorkOperation InsertOperation()
        {
            return _insert;
        }

        public GetWorkOperation GetOperation()
        {
            return _get;
        }

        public bool Contains(string repository, string start, string end)
        {
            using var command = _connection.CreateCommand();

            command.CommandText = @"
                SELECT Id 
                FROM Work 
                WHERE Repository = @Repository 
                AND StartCommit = @StartCommit
                AND EndCommit = @EndCommit";

            command.Parameters.Add(new SqliteParameter("@Repository", SqliteType.Text) {Value = repository});
            command.Parameters.Add(new SqliteParameter("@StartCommit", SqliteType.Text, 50) {Value = start});
            command.Parameters.Add(new SqliteParameter("@EndCommit", SqliteType.Text, 50) {Value = end});

            using var reader = command.ExecuteReader();

            return reader.Read();
        }

        public bool Contains(object id)
        {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT Id FROM Work WHERE Id = @Id";

            command.Parameters.Add(new SqliteParameter("@Id", SqliteType.Integer) {Value = id});

            using var reader = command.ExecuteReader();

            return reader.Read();
        }
    }
}