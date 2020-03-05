// Copyright (c) 2019-present Viktor Semenov
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using DevRating.DefaultObject;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DevRating.SqliteClient.Test
{
    public sealed class SqliteDatabaseWorksTest
    {
        [Fact]
        public void ChecksInsertedWorkById()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                Assert.True(database.Entities().Works().ContainsOperation().Contains(
                        database.Entities().Works().InsertOperation().Insert(
                            "repo",
                            "startCommit",
                            "endCommit",
                            database.Entities().Authors().InsertOperation().Insert("organization", "email").Id(),
                            1u,
                            new DefaultId(),
                            new DefaultEnvelope()
                        ).Id()
                    )
                );
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }

        [Fact]
        public void ChecksInsertedWorkByCommits()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "startCommit",
                    "endCommit",
                    database.Entities().Authors().InsertOperation().Insert("organization", "email").Id(),
                    1u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                Assert.True(database.Entities().Works().ContainsOperation().Contains(
                    "repo",
                    "startCommit",
                    "endCommit"
                ));
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }

        [Fact]
        public void ReturnsInsertedWorkById()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                var work = database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "startCommit",
                    "endCommit",
                    database.Entities().Authors().InsertOperation().Insert("organization", "email").Id(),
                    1u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                Assert.Equal(work.Id(), database.Entities().Works().GetOperation().Work(work.Id()).Id());
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }

        [Fact]
        public void ReturnsInsertedWorkByCommits()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                Assert.Equal(
                    database.Entities().Works().InsertOperation().Insert(
                        "repo",
                        "startCommit",
                        "endCommit",
                        database.Entities().Authors().InsertOperation().Insert("organization", "email").Id(),
                        1u,
                        new DefaultId(),
                        new DefaultEnvelope()
                    ).Id(),
                    database.Entities().Works().GetOperation().Work(
                        "repo",
                        "startCommit",
                        "endCommit"
                    ).Id()
                );
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }

        [Fact]
        public void ReturnsLastInsertedWorkFirst()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "start1",
                    "end1",
                    database.Entities().Authors().InsertOperation().Insert("organization", "author").Id(),
                    1u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                var last = database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "start2",
                    "end2",
                    database.Entities().Authors().InsertOperation().Insert("organization", "other author").Id(),
                    2u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                Assert.Equal(
                    last.Id(),
                    database.Entities().Works().GetOperation().Lasts("repo").First().Id()
                );
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }

        [Fact]
        public void ReturnsFirstInsertedWorkLast()
        {
            var database = new SqliteDatabase(new SqliteConnection("DataSource=:memory:"));

            database.Instance().Connection().Open();
            database.Instance().Create();

            try
            {
                var first = database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "start1",
                    "end1",
                    database.Entities().Authors().InsertOperation().Insert("organization", "author").Id(),
                    1u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                database.Entities().Works().InsertOperation().Insert(
                    "repo",
                    "start2",
                    "end2",
                    database.Entities().Authors().InsertOperation().Insert("organization", "other author").Id(),
                    2u,
                    new DefaultId(),
                    new DefaultEnvelope()
                );

                Assert.Equal(
                    first.Id(),
                    database.Entities().Works().GetOperation().Lasts("repo").Last().Id()
                );
            }
            finally
            {
                database.Instance().Connection().Close();
            }
        }
    }
}