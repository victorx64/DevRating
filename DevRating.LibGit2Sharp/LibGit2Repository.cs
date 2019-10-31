using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevRating.Vcs;
using LibGit2Sharp;
using Commit = DevRating.Vcs.Commit;
using Repository = DevRating.Vcs.Repository;

namespace DevRating.LibGit2Sharp
{
    public sealed class LibGit2Repository : Repository, IDisposable
    {
        private readonly IRepository _repository;
        private readonly string _id;

        public LibGit2Repository(string path, string id) : this(new global::LibGit2Sharp.Repository(path), id)
        {
        }

        public LibGit2Repository(IRepository repository, string id)
        {
            _repository = repository;
            _id = id;
        }

        public async Task WriteInto(ModificationsCollection modifications, string sha)
        {
            var commit = _repository.Lookup<global::LibGit2Sharp.Commit>(sha);

            await Task.WhenAll(WriteCommitTasks(modifications, commit));
        }

        private IEnumerable<Task> WriteCommitTasks(ModificationsCollection modifications,
            global::LibGit2Sharp.Commit commit)
        {
            foreach (var parent in commit.Parents)
            {
                foreach (var task in WriteDifferencesTasks(modifications, commit, parent))
                {
                    yield return task;
                }
            }
        }

        private IEnumerable<Task> WriteDifferencesTasks(
            ModificationsCollection modifications,
            global::LibGit2Sharp.Commit commit,
            global::LibGit2Sharp.Commit parent)
        {
            var differences = _repository.Diff.Compare<Patch>(
                parent.Tree,
                commit.Tree,
                new CompareOptions {ContextLines = 0});

            var options = new BlameOptions {StartingAt = parent};

            foreach (var difference in differences)
            {
                if (!difference.IsBinaryComparison &&
                    difference.OldMode == Mode.NonExecutableFile &&
                    difference.Mode == Mode.NonExecutableFile &&
                    (difference.Status == ChangeKind.Deleted ||
                     difference.Status == ChangeKind.Modified))
                {
                    yield return Task.Run(() =>
                    {
                        var blames = _repository.Blame(difference.OldPath, options);

                        WritePatchInto(modifications, difference.Patch, blames, commit);
                    });
                }
            }
        }

        private void WritePatchInto(
            ModificationsCollection modifications,
            string patch,
            BlameHunkCollection blames,
            global::LibGit2Sharp.Commit commit)
        {
            var author = _repository.Mailmap.ResolveSignature(commit.Author).Email;

            var current = new DefaultCommit(commit.Sha, author, _id);

            foreach (var line in patch.Split('\n'))
            {
                if (line.StartsWith("@@ "))
                {
                    // line must be like "@@ -3,9 +3,9 @@ blah..."
                    // TODO Throw exception on a contextual line. Patch must be without contextual lines (git log -U0)
                    var parts = line.Split(' ');

                    WriteDeletionInto(modifications, current, parts[1], blames);
                    WriteAdditionInto(modifications, current, parts[2]);
                }
            }
        }

        private void WriteDeletionInto(
            ModificationsCollection modifications,
            Commit current,
            string hunk,
            BlameHunkCollection blames)
        {
            var parts = hunk
                .Substring(1)
                .Split(',');

            var index = Convert.ToUInt32(parts[0]) - 1;

            var count = parts.Length == 1 ? 1 : Convert.ToUInt32(parts[1]);

            uint d;

            for (var i = index; i < index + count; i += d)
            {
                var blame = blames.HunkForLine((int) i);
                d = Math.Min((uint) (blame.FinalStartLineNumber + blame.LineCount), index + count) - i;

                var author = _repository.Mailmap.ResolveSignature(blame.FinalSignature).Email;

                var previous = new DefaultCommit(
                    blame.FinalCommit.Sha,
                    author,
                    _id);

                modifications.AddDeletion(new DefaultDeletion(current, previous, d));
            }
        }

        private void WriteAdditionInto(ModificationsCollection modifications, Commit current, string hunk)
        {
            modifications.AddAddition(new DefaultAddition(current, AdditionsCount(hunk)));
        }

        private uint AdditionsCount(string hunk)
        {
            var parts = hunk
                .Substring(1)
                .Split(',');

            var count = parts.Length == 1 ? 1 : Convert.ToUInt32(parts[1]);

            return count;
        }

        public void Dispose()
        {
            _repository.Dispose();
        }
    }
}