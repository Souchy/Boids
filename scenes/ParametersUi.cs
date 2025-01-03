using Boids.Util;
using BoidsProject;
using Godot;
using Godot.Sharp.Extras;
using System;
using System.Linq;
using System.Reflection;

public partial class ParametersUi : Control
{
    #region Nodes
    [NodePath] public Label LblFps { get; set; }
    [NodePath] public GridContainer GridContainer { get; set; }
    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();

        GridContainer.RemoveAndQueueFreeChildren();

        var fields = Parameters.Fields;
        foreach (var field in fields)
        {
            var lbl = new Label();
            lbl.Text = field.Name;
            lbl.Name = field.Name + "Label";
            GridContainer.AddChild(lbl);

            if (field.FieldType == typeof(float))
                AddFieldFloat(field);
            else
            if (field.FieldType == typeof(int))
                AddFieldInt(field);
            else
            if (field.FieldType == typeof(Vector2))
                AddFieldVector2(field);
            else
                GridContainer.AddChild(new Panel());
        }
    }

    private SpinBox MkSpinBox<T>(FieldInfo field)
    {
        return MkSpinBox(field.Name + "Input", Convert.ToDouble(field.GetValue(null)), (val) =>
        {
            field.SetValue(null, Convert.ChangeType(val, typeof(T)));
            EventBus.centralBus.publish(field.Name);
        });
    }
    private SpinBox MkSpinBox(string name, double value, Godot.Range.ValueChangedEventHandler action = null)
    {
        var input = new SpinBox
        {
            Step = 0.1f,
            Rounded = false,
            MaxValue = 2000,
            AllowGreater = true,
            AllowLesser = true,
            Value = value,
            Name = name
        };
        if (action != null) input.ValueChanged += action;
        return input;
    }
    private void AddFieldInt(FieldInfo field)
    {
        var input = MkSpinBox<int>(field);
        input.Step = 1;
        GridContainer.AddChild(input);
    }
    private void AddFieldFloat(FieldInfo field)
    {
        var input = MkSpinBox<float>(field);
        GridContainer.AddChild(input);
    }

    private void AddFieldVector2(FieldInfo field)
    {
        var container = new HBoxContainer();
        container.Name = field.Name + "Input";
        // x
        {
            var input = MkSpinBox("", ((Vector2) field.GetValue(null)).X, (val) =>
            {
                var vec = ((Vector2) field.GetValue(null));
                vec.X = (float) val;
                field.SetValue(null, vec);
                EventBus.centralBus.publish(field.Name);
            });
            container.AddChild(input);
        }
        // y
        {
            var input = MkSpinBox("", ((Vector2) field.GetValue(null)).Y, (val) =>
            {
                var vec = ((Vector2) field.GetValue(null));
                vec.Y = (float) val;
                field.SetValue(null, vec);
                EventBus.centralBus.publish(field.Name);
            });
            container.AddChild(input);
        }
        GridContainer.AddChild(container);
    }

    public override void _Process(double delta)
    {
        LblFps.Text = Engine.GetFramesPerSecond() + " fps";
    }

}
