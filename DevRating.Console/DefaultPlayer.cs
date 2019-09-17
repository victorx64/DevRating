using System.Collections.Generic;
using System.Linq;
using DevRating.Game;

namespace DevRating.Console
{
    public sealed class DefaultPlayer : Player
    {
        private readonly IList<Game> _games;

        public DefaultPlayer(Game game) : this(new List<Game> {game})
        {
        }

        public DefaultPlayer(IList<Game> games)
        {
            _games = games;
        }

        public Player PerformedPlayer(string contender, string commit, double points, double reward, int rounds)
        {
            return new DefaultPlayer(
                new List<Game>(_games)
                {
                    new DefaultGame(contender, commit, points, reward, rounds)
                });
        }

        public double Points()
        {
            return _games.Last().PointsAfter();
        }
    }
}