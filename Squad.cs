using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace KingdomInvader
{
    public partial class Squad : Area2D
    {
        public Vector2 Destination = Vector2.Zero;
        public int Population;
        public Color Color;
        public Vector2 Size;

        private CollisionShape2D collisionShape;
        private ColorRect colorRect;
        private float speed = 100; // pixels per second
        private Label squadLabel;
        private bool moving;
        private bool dragging = false;
        private Vector2 dragStart;
        private bool merged = false;

        public override void _Ready()
        {
            AddToGroup("squad");

            collisionShape = new CollisionShape2D();
            AddChild(collisionShape);

            collisionShape.Shape = new RectangleShape2D { Size = Size };

            SetProcessInput(true);
            SetProcess(true);
            SetPhysicsProcess(true);
            Monitoring = true;
            Monitorable = true;

            moving = true;

            squadLabel = new Label();

            colorRect = new ColorRect
            {
                Size = Size,
                Color = Color
            };
            AddChild(colorRect);
            AddChild(squadLabel);

            Connect("area_entered", new Callable(this, nameof(OnSquadEntered)));
        }

        public override void _Process(double delta)
        {
            squadLabel.Text = $"{Population}";
            QueueRedraw();
        }

        // Draw an arrow from the origin to the current mouse position QL
        public override void _Draw()
        {
            if (dragging)
            {
                DrawLine(dragStart, GetLocalMousePosition(), Colors.Red);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (Destination != Vector2.Zero)
            {
                Vector2 direction = (Destination - Position - new Vector2(Size.X / 2, Size.Y / 2)).Normalized();
                float distanceToDestination = (Destination - Position - new Vector2(Size.X / 2, Size.Y / 2)).Length();

                // If the Squad is close enough to its destination, stop moving to prevent rounding issues which causes the shape to shake
                if (distanceToDestination > speed * delta)
                {
                    moving = true;
                    Position += direction * speed * (float)delta;
                }
                else
                {
                    moving = false;
                    Position = Destination - new Vector2(Size.X / 2, Size.Y / 2);
                }
            }
            
        }

        private void OnSquadEntered(Area2D area)
        {
            if (moving && !merged && area is Squad otherSquad && !otherSquad.merged)
            {
                // Merge the squads and remove the inactive
                // If both squads move and collide its a bit random - perhaps we should prevent merging if both squads are moving TODO
                Population += otherSquad.Population;
                otherSquad.merged = true;
                otherSquad.QueueFree();
            }
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
                        dragging = true;
                        dragStart = GetLocalMousePosition();
                    }
                }
                else if (!mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    if (dragging)
                    {
                        dragging = false;
                        Vector2 destination = Position + GetLocalMousePosition() - dragStart;
                        Destination = destination + new Vector2(Size.X / 2, Size.Y / 2);
                    }
                }
                else if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Right)
                {
                    if (colorRect.GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                    {
                        dragging = true;
                        dragStart = GetLocalMousePosition();
                    }
                }
                else if (!mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Right)
                {
                    if (dragging)
                    {
                        var leftoverPop = (int)Math.Round(Population * (GameState.UnitSliderValue / 100));
                        // Leave whatever many troops behind
                        var squad = new Squad
                        {
                            Position = GlobalPosition,
                            Color = Color,
                            Size = Size,
                            Population = Population - leftoverPop,
                        };
                        merged = true;

                        var timer = new Timer();
                        timer.WaitTime = 0.1f;
                        timer.OneShot = true;
                        AddChild(timer);
                        timer.Start();
                        timer.Connect("timeout", new Callable(this, nameof(OnSplitMove)));

                        Population = leftoverPop;
                        GetTree().Root.AddChild(squad);

                        // Stop dragging
                        dragging = false;
                        // Save the drop position
                        Vector2 destination = Position + GetLocalMousePosition() - dragStart;
                        Destination = destination + new Vector2(Size.X / 2, Size.Y / 2);
                    }
                }
            }
        }

        private void OnSplitMove()
        {
            merged = false;
        }
    }
}
