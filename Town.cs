using Godot;
using System;

namespace KingdomInvader
{
    public partial class Town : Area2D
    {
        public int GowthPerSecond;
        public int Population;
        public Player PlayerOwner;
        public Vector2 Size;

        private bool dragging = false;
        private Vector2 dragStart;
        private Vector2 dropPosition;
        private double frameCounter;
        private Label townLabel;
        private ColorRect colorRect;
        private CollisionShape2D collisionShape;

        public override void _Ready()
        {
            AddToGroup("town");
            SetProcessInput(true);
            SetProcess(true);
            SetPhysicsProcess(true);

            collisionShape = new CollisionShape2D();
            AddChild(collisionShape);

            collisionShape.Shape = new RectangleShape2D { Size = Size };

            colorRect = new ColorRect();
            colorRect.Color = PlayerOwner.Color;
            colorRect.Size = Size;
            AddChild(colorRect);

            // Create a new Label and add it as a child of this Town
            townLabel = new Label();
            AddChild(townLabel);
        }

        public override void _PhysicsProcess(double delta)
        {
            frameCounter += delta;

            if (frameCounter >= 1)
            {
                Population += GowthPerSecond;
                frameCounter = 0;
            }
            base._PhysicsProcess(delta);
        }

        public override void _Process(double delta)
        {
            townLabel.Text = $"Pop: {Population}";
            QueueRedraw();
        }

        public override void _Draw()
        {
            if (dragging)
            {
                // Draw an arrow from the origin to the current mouse position
                DrawLine(dragStart, GetLocalMousePosition(), Colors.Red);
            }
        }

        public void AddPopulationFromSquad(Squad squad)
        {
            Population += squad.Population;
            squad.Population = 0;
            squad.QueueFree();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    // Check if the mouse event occurred within this DragRect
                    if (colorRect.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                    {
                        // Start dragging
                        dragging = true;
                        dragStart = GetLocalMousePosition();
                    }
                }
                else if (!mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    // Check if the mouse event occurred within this DragRect
                    if (dragging && Population >= 1)
                    {
                        // Stop dragging
                        dragging = false;
                        // Save the drop position
                        dropPosition = GetLocalMousePosition();

                        var outgoingPop = (int)Math.Round(Population * (GameState.UnitSliderValue / 100));
                        if (outgoingPop == 0) outgoingPop = 1;
                        var remainingPop = Population - outgoingPop;

                        /*if (outgoingPop >= GameSettings.SquadMaxPopulation)
                        {
                            var overhang = outgoingPop - GameSettings.SquadMaxPopulation;
                            remainingPop += overhang;
                            outgoingPop = GameSettings.SquadMaxPopulation;
                        }*/

                        // release the army
                        var squad = new Squad()
                        {
                            PlayerOwner = PlayerOwner,
                            Position = dragStart + Position,
                            Destination = dropPosition + Position,
                            Population = outgoingPop
                        };
                        GameState.MapNode.AddChild(squad);
                        Population = remainingPop;
                    }
                }
            }
        }
    }
}
