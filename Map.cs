using Godot;
using System.Collections.Generic;

namespace KingdomInvader
{
    // TODO perhaps its better to call it map generator
    public partial class Map : Node2D
    {
        public int sizeX;
        public int sizeY;

        private List<Town> towns { get; set; } = new List<Town>();
        private List<Player> players { get; set; } = new List<Player>();

        public override void _Ready()
        {
            SetProcessInput(true);
            SetProcess(true);

            var player = new Player()
            {
                Color = Colors.Blue
            };

            var playerEnemy = new Player()
            {
                Color = Colors.Red
            };

            var playerNeutral = new Player()
            {
                Color = Colors.Green
            };

            players.Add(player);
            players.Add(playerEnemy);
            players.Add(playerNeutral);

            var town = new Town()
            {
                PlayerOwner = player,
                Position = new Vector2(100, 100),
                Size = new Vector2(100, 100),
                GowthPerSecond = 1,
                Population = 100
            };

            var townEnemy = new Town()
            {
                PlayerOwner = playerEnemy,
                Position = new Vector2(400, 400),
                Size = new Vector2(100, 100),
                GowthPerSecond = 1,
                Population = 100
            };

            var townNeutral = new Town()
            {
                PlayerOwner = playerNeutral,
                Position = new Vector2(200, 400),
                Size = new Vector2(100, 100),
                GowthPerSecond = 1,
                Population = 100
            };

            AddChild(town);
            towns.Add(town);

            AddChild(townEnemy);
            towns.Add(townEnemy);

            AddChild(townNeutral);
            towns.Add(townNeutral);
        }
    }
}
