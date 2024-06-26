using Godot;

namespace KingdomInvader
{
    partial class GameState : Node
    {
        public static GameSettings Settings = new();
        public static float UnitSliderValue { get; set; }
        public static Map MapNode { get; set; }
    }
}
