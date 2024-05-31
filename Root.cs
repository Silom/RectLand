using Godot;
using KingdomInvader;
using System;
using System.Collections.Generic;

public partial class Root : Node2D
{
    private List<Town> towns { get; set; } = new List<Town>();
    private HSlider slider;

    public override void _Ready()
    {
        var initalSliderValue = 50;
        slider = new HSlider
        {
            MinValue = 1,
            MaxValue = 100,
            Step = 1,
            Value = initalSliderValue,
            Size = new Vector2(200, 20),
            AnchorTop = 0.0f,
            AnchorLeft = 0.5f,
            /*MarginTop = 10.0f,
            MarginLeft = -100.0f*/
        };
        GameState.UnitSliderValue = 50;
        AddChild(slider);
        slider.Connect("value_changed", new Callable(this, nameof(OnSliderValueChanged)));

        SetProcessInput(true);
        SetProcess(true);

        var town = new Town()
        {
            Color = Colors.Pink,
            Position = new Vector2(200, 200),
            Size = new Vector2(100, 100),
            GowthPerSecond = 1,
            Population = 100
        };
        AddChild(town);
        towns.Add(town);
    }

    private void OnSliderValueChanged(float value)
    {
        GameState.UnitSliderValue = value;
        GD.Print($"Slider value: {value}%");
    }

    public override void _Process(double delta)
	{
    }
}
