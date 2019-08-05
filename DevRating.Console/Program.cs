﻿using System.Collections.Generic;
using DevRating.Git;
using DevRating.Rating;

namespace DevRating.Console
{
    internal static class Program
    {
        private static void Main(string[] arguments)
        {
            new Report(
                    new Git.Git(
                        new Dictionary<string, IPlayer>(),
                        new Player(
                            new Elo()),
                        new File()),
                    new OutputChannels(
                        arguments,
                        new QuiteConsoleOutput(),
                        new VerboseConsoleOutput()))
                .Print();
        }
    }
}