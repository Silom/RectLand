using Godot;
using KingdomInvader.Models;

namespace KingdomInvader
{
    public partial class Root : Node2D
    {
        GameContext context = new GameContext();
        private Control uiControl;
        private HSlider slider;
        private Label sliderLabel;
        private Map currentMap;
        private Camera2D camera;

        public override void _Ready()
        {

            SetProcessInput(true);
            SetProcess(true);

            uiControl = new Control();
            AddChild(uiControl);

            RenderMap();

            camera = new Camera2D();
            var viewportSize = GetViewportRect().Size;
            // center the cam a little bit in order to not have the edge of the map visiable right away
            camera.GlobalPosition = new Vector2(viewportSize.X / 2, viewportSize.Y / 2);
            currentMap.AddChild(camera);

            InitializeIngameUI();

            GetViewport().Connect("size_changed", new Callable(this, nameof(UpdateUIPosition)));
        }


        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                if (!mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Middle))
                {
                    return;
                }

                var cameraPosition = camera.GlobalPosition;
                cameraPosition.X -= mouseMotion.Relative.X;
                cameraPosition.Y -= mouseMotion.Relative.Y;
                camera.GlobalPosition = cameraPosition;
            }
            else if (@event is InputEventMouseButton mouseButton)
            {
                // scaling the map in order to leave the ui components in place
                // TODO should likly be done for the map position as well rather than translating the controls
                if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                {
                    var zoomLevel = Mathf.Max(currentMap.Scale.X - 0.1f, 0.1f);
                    currentMap.Scale = new Vector2(zoomLevel, zoomLevel);
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                {
                    var zoomLevel = Mathf.Min(currentMap.Scale.X + 0.1f, 5.0f);
                    currentMap.Scale = new Vector2(zoomLevel, zoomLevel);
                }
            }

            UpdateUIPosition();
        }

        private void RenderMap()
        {
            currentMap = new Map();
            AddChild(currentMap);
            GameState.MapNode = currentMap;
        }

        private void InitializeIngameUI()
        {
            var initalSliderValue = 50;
            slider = new HSlider
            {
                MinValue = 1,
                MaxValue = 100,
                Step = 1,
                Value = initalSliderValue,
                Size = new Vector2(300, 30),
            };
            GameState.UnitSliderValue = 50;
            uiControl.AddChild(slider);
            slider.Connect("value_changed", new Callable(this, nameof(OnSliderValueChanged)));

            sliderLabel = new Label()
            {
                Text = $"{initalSliderValue}%"
            };
            uiControl.AddChild(sliderLabel);

            UpdateUIPosition();
        }

        private void UpdateUIPosition()
        {
            var viewportSize = GetViewportRect().Size;
            GD.PushError(viewportSize);

            // 0,0 + cam: will be the center of the screen with the offset element size
            slider.Position = new Vector2(-150, -(viewportSize.Y / 2)) + camera.GlobalPosition;
            sliderLabel.Position = new Vector2(-15, -(viewportSize.Y / 2) + 25) + camera.GlobalPosition;
        }

        private void OnSliderValueChanged(float value)
        {
            GameState.UnitSliderValue = value;
            sliderLabel.Text = $"{value}%";
        }
    }
}