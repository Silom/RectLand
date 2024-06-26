using Godot;
using System;

namespace KingdomInvader
{
    public partial class Squad : Area2D
    {
        public Vector2 Destination = Vector2.Zero;
        public bool MovementBlocked = false;
        public int Population;
        public Player PlayerOwner;
        public Combat InvolvedCombat;
        public Vector2 Size = new Vector2(30, 30);
        private CollisionShape2D collisionShape;
        private ColorRect colorRect;
        private float speed = 100; // pixels per second
        private Label squadLabel;
        private bool moving;
        private bool dragging = false;
        private Vector2 dragStart;
        private bool merged = false;

        private Node2D animatedSpriteContainer;

        public override void _Ready()
        {
            AddToGroup("squad");

            var newSize = CalculateRectSize();
            Size = new Vector2(newSize, newSize);

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
            squadLabel.Position += new Vector2(0, -30);
            colorRect = new ColorRect
            {
                Size = Size,
                Color = PlayerOwner.Color
            };
            AddChild(colorRect);
            AddChild(squadLabel);

            animatedSpriteContainer = (Node2D)ResourceLoader.Load<PackedScene>("res://AnimatedSprite.tscn").Instantiate();

            int rows = (int)Math.Ceiling(Math.Sqrt(Population));
            int columns = (int)Math.Ceiling((double)Population / rows);
            float spacing = Math.Min(Size.X / columns, Size.Y / rows);

            for (int i = 0; i < Population; i++)
            {
                var sprite = (Node2D)animatedSpriteContainer.Duplicate();// TODO Full dup so we only have to animate one sprite
                int row = i / columns;
                int column = i % columns;
                sprite.Position = new Vector2(Size.X / 2 - (columns / 2 - column) * spacing, Size.Y / 2 - (rows / 2 - row) * spacing);
                AddChild(sprite);
            }

            animatedSpriteContainer.Position = new Vector2(Size.X / 2, Size.Y / 2);

            Connect("area_entered", new Callable(this, nameof(OnSquadEntered)));
        }

        public override void _Process(double delta)
        {
            var animatedSprite = (AnimatedSprite2D)animatedSpriteContainer.FindChild("AnimatedSprite");
            if (animatedSprite != null)
            {
                if (moving)
                {
                    animatedSprite.Play("walk_down");
                }
                else
                {
                    animatedSprite.Stop();
                }
            }


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
            ApplyRectSize();
        }

        private int CalculateRectSize()
        {
            double sizeRatio = (double)Population / (double)GameSettings.TownMaxPopulation;// adjust size of army to relative to size of town
            double newSize = GameSettings.SquadMaxSize * sizeRatio;

            if (GameSettings.SquadMinSize > newSize)
            {
                return GameSettings.SquadMinSize;
            }
            if (GameSettings.SquadMaxSize < newSize)
            {
                return GameSettings.SquadMaxSize;
            }
            return (int)newSize;
        }

        // maintain the collision shape as well as the rect area
        private void ApplyRectSize()
        {
            var newSize = CalculateRectSize();
            Size = new Vector2(newSize, newSize);
            collisionShape.Shape = new RectangleShape2D { Size = Size };
            colorRect.Size = Size;
        }

        private void OnSquadEntered(Area2D area)
        {
            GD.PushError("Squad entered Area");
            GD.PushError(area);

            if (area is Squad otherSquad)
            {
                if (moving && !merged && !otherSquad.merged)
                {
                    if (otherSquad.InvolvedCombat != null)
                    {
                        InvolvedCombat = otherSquad.InvolvedCombat;
                        InvolvedCombat.JoinCombat(this);
                    }
                    else if (otherSquad.PlayerOwner != PlayerOwner)
                    {
                        // Initiate combat
                        StartCombat(otherSquad);
                    }
                    else
                    {
                        // Merge the squads and remove the inactive
                        // If both squads move and collide its a bit random - perhaps we should prevent merging if both squads are moving TODO
                        Population += otherSquad.Population;
                        otherSquad.merged = true;
                        otherSquad.QueueFree();
                    }
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (MovementBlocked)
            {
                return;
            }

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
                            Position = Position,
                            PlayerOwner = PlayerOwner,
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
                        GameState.MapNode.AddChild(squad);
                        dragging = false;
                        // Save the drop position
                        Vector2 destination = Position + GetLocalMousePosition() - dragStart;
                        Destination = destination + new Vector2(Size.X / 2, Size.Y / 2);
                    }
                }
            }
        }

        private void StartCombat(Squad otherSquad)
        {
            if (otherSquad.InvolvedCombat != null)
            {
                InvolvedCombat = otherSquad.InvolvedCombat;
            }
            else if (InvolvedCombat != null)
            {
                otherSquad.InvolvedCombat = InvolvedCombat;
            }
            else
            {
                InvolvedCombat = new Combat()
                {
                    Squads = new System.Collections.Generic.List<Squad> { this, otherSquad }
                };
                GameState.MapNode.AddChild(InvolvedCombat);
                otherSquad.InvolvedCombat = InvolvedCombat;
            }

        }

        private void OnSplitMove()
        {
            merged = false;
        }
    }
}
