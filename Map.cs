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

        public override void _Ready()
        {
            SetProcessInput(true);
            SetProcess(true);

            var town = new Town()
            {
                Color = Colors.Pink,
                Position = new Vector2(200, 200),
                Size = new Vector2(100, 100),
                GowthPerSecond = 1,
                Population = 100
            };
            GD.PushError("add city");
            AddChild(town);
            towns.Add(town);
        }
    }
}
