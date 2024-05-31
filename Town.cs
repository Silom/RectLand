using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomInvader
{
    public partial class Town : ColorRect
    {
        private bool dragging = false;
        private Vector2 dragStart;
        private Vector2 dropPosition;
        private double frameCounter;

        private Label townLabel;

        public int GowthPerSecond { get; internal set; }
        public int Population { get; internal set; }

        public override void _Ready()
        {
            AddToGroup("town");
            this.SetProcessInput(true);
            this.SetProcess(true);
            this.SetPhysicsProcess(true);

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
                            Position = dragStart,
                            Destination = GetLocalMousePosition(),
                            Population = outgoingPop
                        };
                        AddChild(squad);
                        Population = remainingPop;
                    }
                }
            }
        }
    }
}
