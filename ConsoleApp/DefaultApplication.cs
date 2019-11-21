using System;
using System.Collections.Generic;
using DevRating.Database;
using DevRating.Domain;
using DevRating.LibGit2SharpClient;

namespace DevRating.ConsoleApp
{
    internal sealed class DefaultApplication : Application
    {
        private readonly Arguments _arguments;
        private readonly Instance _instance;
        private readonly IDictionary<string, Action> _actions;

        public DefaultApplication(Arguments arguments, Instance instance)
        {
            _arguments = arguments;
            _instance = instance;
            _actions = new Dictionary<string, Action>
            {
                {"show", PrintToConsole},
                {"load", PrintSavedToConsole},
                {"save", Save},
                {"reset", Reset},
                {"top", Top}
            };
        }

        public void Run()
        {
            _actions[_arguments.Command()].Invoke();
        }

        private Diff Diff()
        {
            return new LibGit2Diff(_arguments.Path(), _arguments.StartCommit(), _arguments.EndCommit());
        }

        private void Top()
        {
            var connection = _instance.Connection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var author in _instance.Authors().TopAuthors())
                {
                    Console.WriteLine($"{author.Email()} {author.Rating().Value():F2}");
                }
            }
            finally
            {
                transaction.Rollback();
                connection.Close();
            }
        }

        private void Reset()
        {
            var connection = _instance.Connection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                if (_instance.Exist())
                {
                    _instance.Drop();
                }

                _instance.Create();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                connection.Close();
            }
        }

        private void PrintToConsole()
        {
            var connection = _instance.Connection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                Diff().AddTo(_instance.Works());

                PrintWorkToConsole(_instance.Works().Work(Diff().Key()));
            }
            finally
            {
                transaction.Rollback();
                connection.Close();
            }
        }

        private void PrintSavedToConsole()
        {
            var connection = _instance.Connection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                PrintWorkToConsole(_instance.Works().Work(Diff().Key()));
            }
            finally
            {
                transaction.Rollback();
                connection.Close();
            }
        }

        private void PrintWorkToConsole(Work work)
        {
            Console.WriteLine("Reward:");
            Console.WriteLine($"{work.Author().Email()} {work.Reward():F2}");
            Console.WriteLine("Rating updates:");

            foreach (var rating in work.Ratings())
            {
                Console.WriteLine(rating.HasPreviousRating()
                    ? $"{rating.Author().Email()} {rating.PreviousRating().Value():F2} -> {rating.Value():F2}"
                    : $"{rating.Author().Email()} {rating.Value():F2}");
            }
        }

        private void Save()
        {
            var connection = _instance.Connection();

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                Diff().AddTo(_instance.Works());

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}