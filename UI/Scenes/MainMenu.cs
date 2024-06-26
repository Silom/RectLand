using Godot;

namespace KingdomInvader.UI.Scenes
{
    public partial class MainMenu : Control
    {
        private Control AnimatedBackground;
        private Button ButtonPlay;
        private Button ButtonSettings;
        private Button ButtonExit;

        public override void _Ready()
        {
            SetProcess(true);
            AnimatedBackground = GetNode<Control>("BackgroundAnimated");
            ButtonPlay = GetNode<Button>("%ButtonPlay");
            ButtonSettings = GetNode<Button>("%ButtonSettings");
            ButtonExit = GetNode<Button>("%ButtonExit");

            ButtonPlay.Connect("pressed", new Callable(this, nameof(OnButtonPlayPressed)));
            ButtonSettings.Connect("pressed", new Callable(this, nameof(OnButtonSettingsPressed)));
            ButtonExit.Connect("pressed", new Callable(this, nameof(OnButtonExitPressed)));

        }

        public override void _Process(double delta)
        {
            var vp = GetViewportRect();
            var mp = GetGlobalMousePosition();
            var tween = AnimatedBackground.CreateTween();
            var offsetX = vp.Size.X / 2 - mp.X * 0.8;
            var offsetY = vp.Size.Y / 2 - mp.Y * 0.8;
            tween.TweenProperty(AnimatedBackground, "position", new Vector2((float)offsetX, (float)offsetY), 1.0);
        }

        private void OnButtonPlayPressed()
        {
            GetTree().ChangeSceneToFile("res://Root.tscn");
            //QueueFree(); TODO check if ChangeSceneToFile will unload the scene properly
        }

        private void OnButtonSettingsPressed()
        {
            // TODO settings should be added to the viewport when pressed
            // Handle Settings button press event
            GD.Print("Settings button pressed");
        }

        private void OnButtonExitPressed()
        {
            // Handle Exit button press event
            GetTree().Quit();
        }
    }
}
