// Copyright (c) 2019-present Viktor Semenov
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using DevRating.DefaultObject;
using DevRating.Domain;

namespace DevRating.ConsoleApp
{
    public sealed class ConsoleApplication : Application
    {
        private readonly Database _database;
        private readonly Formula _formula;

        public ConsoleApplication(Database database, Formula formula)
        {
            _database = database;
            _formula = formula;
        }

        public void Top(Console console, string organization)
        {
            _database.Instance().Connection().Open();

            using var transaction = _database.Instance().Connection().BeginTransaction();

            try
            {
                if (!_database.Instance().Present())
                {
                    _database.Instance().Create();
                }

                foreach (var author in _database.Entities().Authors().GetOperation().TopOfOrganization(organization))
                {
                    var percentile = _formula
                        .WinProbabilityOfA(
                            _database.Entities().Ratings().GetOperation().RatingOf(author.Id()).Value(),
                            _formula.DefaultRating()
                        );

                    console.WriteLine(
                        $"{author.Email()} " +
                        $"{_database.Entities().Ratings().GetOperation().RatingOf(author.Id()).Value():F2} " +
                        $"({percentile:P} percentile)"
                    );
                }
            }
            finally
            {
                transaction.Rollback();
                _database.Instance().Connection().Close();
            }
        }

        public void Save(Diff diff)
        {
            _database.Instance().Connection().Open();

            using var transaction = _database.Instance().Connection().BeginTransaction();

            try
            {
                if (!_database.Instance().Present())
                {
                    _database.Instance().Create();
                }

                if (diff.PresentIn(_database.Entities().Works()))
                {
                    throw new InvalidOperationException("The diff is already added.");
                }

                diff.AddTo(new DefaultEntityFactory(_database.Entities(), _formula));

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                _database.Instance().Connection().Close();
            }
        }

        public void PrintTo(Console console, Diff diff)
        {
            _database.Instance().Connection().Open();

            using var transaction = _database.Instance().Connection().BeginTransaction();

            try
            {
                if (!_database.Instance().Present())
                {
                    _database.Instance().Create();
                }

                if (!diff.PresentIn(_database.Entities().Works()))
                {
                    diff.AddTo(new DefaultEntityFactory(_database.Entities(), _formula));

                    console.WriteLine("To add these updates run `devrating add <path> <before> <after>`.");
                    console.WriteLine();
                }

                PrintWorkToConsole(console, diff.From(_database.Entities().Works()));
            }
            finally
            {
                transaction.Rollback();
                _database.Instance().Connection().Close();
            }
        }

        private void PrintWorkToConsole(Console console, Work work)
        {
            var usedRating = work.UsedRating();

            var rating = usedRating.Id().Filled()
                ? usedRating.Value()
                : _formula.DefaultRating();

            var percentile = _formula.WinProbabilityOfA(rating, _formula.DefaultRating());

            console.WriteLine(work.Author().Email());
            console.WriteLine($"Added {work.Additions()} lines with {rating} rating ({percentile:P} percentile)");
            console.WriteLine(
                $"Reward = {work.Additions()} / (1 - {percentile:F2}) = {work.Additions() / (1d - percentile):F2}");
            console.WriteLine();

            PrintWorkRatingsToConsole(console, work);
        }

        private void PrintWorkRatingsToConsole(Console console, Work work)
        {
            console.WriteLine("Rating updates");

            if (work.Since().Filled())
            {
                console.WriteLine($"The current major version starts at {work.Since().Value()}");
                console.WriteLine("Old lines are ignored");
                console.WriteLine();
            }

            foreach (var rating in _database.Entities().Ratings().GetOperation().RatingsOf(work.Id()))
            {
                var percentile = _formula.WinProbabilityOfA(rating.Value(), _formula.DefaultRating());

                var previous = rating.PreviousRating();

                var before = previous.Id().Filled()
                    ? previous.Value()
                    : _formula.DefaultRating();

                var deletions = rating.CountedDeletions();

                var information = deletions.Filled()
                    ? $"lost {deletions.Value()} lines"
                    : "the performer";

                console.WriteLine($"{rating.Author().Email()} {before:F2} " +
                                  $"({information}) -> {rating.Value():F2} ({percentile:P} percentile)");
            }
        }
    }
}