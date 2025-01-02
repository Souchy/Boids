using BoidsProject;
using Godot;
using Godot.Sharp.Extras;
using System;
using System.Linq;

public partial class ParametersUi : Control
{
    #region Nodes
    [NodePath] public Label LblFps { get; set; }
    [NodePath] public GridContainer GridContainer { get; set; }
    //[NodePath] public SpinBox ValueCohesion {  get; set; }
    //[NodePath] public SpinBox ValueAlignment { get; set; }
    //[NodePath] public SpinBox ValueSeparation { get; set; }
    //[NodePath] public SpinBox ValueBoundAvoidanceWeight { get; set; }
    //[NodePath] public SpinBox ValueObstacleAvoidanceWeight { get; set; }
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
            {
                var input = new SpinBox();
                input.Rounded = false;
                input.AllowGreater = true;
                input.AllowLesser = true;
                input.Value = (float) field.GetValue(null);
                input.Name = field.Name + "Input";
                input.ValueChanged += (val) => field.SetValue(null, (float) val);
                GridContainer.AddChild(input);
            }
            else
            if (field.FieldType == typeof(int))
            {
                var input = new SpinBox();
                input.Rounded = false;
                input.AllowGreater = true;
                input.AllowLesser = true;
                input.Value = (int) field.GetValue(null);
                input.Name = field.Name + "Input";
                input.ValueChanged += (val) => field.SetValue(null, (int) val);
                GridContainer.AddChild(input);
            }
            else
            if (field.FieldType == typeof(Vector2))
            {
                var container = new HBoxContainer();
                container.Name = field.Name + "Input";
                {
                    var inputX = new SpinBox();
                    inputX.Rounded = false;
                    inputX.Value = ((Vector2) field.GetValue(null)).X;
                    inputX.ValueChanged += (val) =>
                    {
                        var vec = ((Vector2) field.GetValue(null));
                        vec.X = (float) val;
                        field.SetValue(null, vec);
                    };
                    container.AddChild(inputX);
                }
                {
                    var inputY = new SpinBox();
                    inputY.Rounded = false;
                    inputY.Value = ((Vector2) field.GetValue(null)).Y;
                    inputY.ValueChanged += (val) =>
                    {
                        var vec = ((Vector2) field.GetValue(null));
                        vec.Y = (float) val;
                        field.SetValue(null, vec);
                    };
                    container.AddChild(inputY);
                }
                GridContainer.AddChild(container);
            }
            else
            {
                GridContainer.AddChild(new Panel());
            }
        }
    }

    public override void _Process(double delta)
    {
        LblFps.Text = Engine.GetFramesPerSecond() + " fps";
    }

}
