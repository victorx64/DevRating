﻿using System;
using System.Collections.Generic;
using System.Linq;
using DevRating.Rating;
using LibGit2Sharp;

namespace DevRating.VersionControlSystem.Git
{
    public class Git : IVersionControlSystem
    {
        private readonly IRating _rating;

        public Git(IRating rating)
        {
            _rating = rating;
        }

        public IRating UpdatedRating()
        {
            var rating = _rating;

            var files = new Dictionary<string, File>();

            using (var repo = new Repository("."))
            {
                var options = new CompareOptions()
                {
                    ContextLines = 0
                };

                var filter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Topological |
                             CommitSortStrategies.Reverse
                };

                var index = 0;
                var count = repo.Commits.QueryBy(filter).Count();

                Tree tree = null;

                foreach (var current in repo.Commits.QueryBy(filter))
                {
                    Console.WriteLine($"{++index} / {count}");

                    var differences = repo.Diff.Compare<Patch>(tree, current.Tree, options);

                    rating = UpdateRating(files, current.Author.Email, differences, rating);

                    tree = current.Tree;
                }
            }

            return rating;
        }

        private IRating UpdateRating(IDictionary<string, File> files, string author, Patch differences, IRating rating)
        {
            var before = new Dictionary<string, File>(files);

            foreach (var difference in differences)
            {
                if (difference.Status == ChangeKind.Added ||
                    difference.Status == ChangeKind.Copied ||
                    difference.Status == ChangeKind.Deleted ||
                    difference.Status == ChangeKind.Modified ||
                    difference.Status == ChangeKind.Renamed)
                {
                    if (difference.Status != ChangeKind.Added &&
                        difference.Status != ChangeKind.Copied)
                    {
                        files.Remove(difference.OldPath);
                    }

                    if (difference.Status != ChangeKind.Deleted)
                    {
                        var previous = difference.Status == ChangeKind.Added
                            ? new File(new string[0], false)
                            : before[difference.OldPath];

                        var file = UpdateFile(previous, author, difference.Patch, difference.IsBinaryComparison);

                        rating = file.UpdateRating(rating);

                        files.Add(difference.Path, file);
                    }
                }
                else
                {
                    ; // TODO Handle other statuses, e.g. Submodules
                }
            }

            return rating;
        }

        private File UpdateFile(File previous, string author, string patch, bool binaryComparison)
        {
            var file = new File(previous.Authors(), previous.Binary() || binaryComparison);

            if (!binaryComparison)
            {
                var lines = patch.Split('\n');

                foreach (var line in lines)
                {
                    if (line.StartsWith("@@ "))
                    {
                        var parts = line.Split(' ');

                        file.AddDeletion(new LinesBlock(author, parts[1]));
                        file.AddAddition(new LinesBlock(author, parts[2]));
                    }
                }
            }

            return file;
        }
    }
}