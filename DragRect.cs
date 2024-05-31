using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingdomInvader
{
    public partial class DragRect : ColorRect
    {
        private bool dragging = false;
        private Vector2 dragStart;

        public override void _Ready()
        {
            AddToGroup("draggable");
            SetProcessInput(true);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                    {
                        dragging = true;
                        dragStart = mouseButtonEvent.Position;
                        GD.PushError("start dragging");
                    }
                }
                else if (!mouseButtonEvent.Pressed && mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    dragging = false;

                    // Get all nodes in the "draggable" group
                    var otherRects = GetTree().GetNodesInGroup("draggable").Where(node => node is DragRect && node != this).ToList();

                    // Check if the global rectangle of the current instance intersects with the global rectangle of any other instance
                    foreach (DragRect rect in otherRects)
                    {
                        if (GetGlobalRect().Intersects(rect.GetGlobalRect()))
                        {
                            GD.PushError("rect was dropped onto another rect");
                            break;
                        }
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotionEvent)
            {
                if (dragging)
                {
                    // Calculate the new position
                    Vector2 newPosition = Position + mouseMotionEvent.Relative;
                    Position = newPosition;
                }
            }
        }
    }
}
