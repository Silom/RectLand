using Godot;

namespace KingdomInvader
{
    partial class GameSettings : Node
    {
        public static int SquadMinSize { get; set; } = 30;
        public static int SquadMaxSize { get; set; } = 70;
        /*        public static int SquadMaxPopulation { get; set; } = 300;*/
        public static int TownMaxPopulation { get; set; } = 1000;
        public static int TownMaxSupply { get; set; } = 1000;
        public static int TownSize { get; set; } = 100; // start of game
    }
}
