using Godot;
using System;

namespace KingdomInvader
{
    public partial class Town : ColorRect
    {
        private bool dragging = false;
        private Vector2 dragStart;
        private Vector2 dropPosition;
        private double frameCounter;

        private Label townLabel;

        public int GowthPerSecond;
        public int Population;

        public override void _Ready()
        {
            AddToGroup("town");
            SetProcessInput(true);
            SetProcess(true);
            SetPhysicsProcess(true);

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
                GD.PushError("DrawLine");
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    // Check if the mouse event occurred within this DragRect
                    if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
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

                        // release the army
                        var squad = new Squad()
                        {
                            Size = new Vector2(30, 30),
                            Color = Colors.Red,
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
