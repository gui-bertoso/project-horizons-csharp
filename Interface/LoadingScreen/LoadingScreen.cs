using Godot;

public partial class LoadingScreen : CanvasLayer
{
    public static LoadingScreen I { get; private set; }

    private Label _label;
    private Label _subLabel;
    private ProgressBar _progressBar;

    public override void _Ready()
    {
        I = this;

        _label = GetNode<Label>("Control/MainLabel");
        _subLabel = GetNode<Label>("Control/SubLabel");
        _progressBar = GetNode<ProgressBar>("Control/ProgressBar");

        HideLoading();
    }

    public void ShowLoading(string text = "loading...", float progress = 0f)
    {
        Visible = true;
        SetText(text);
        SetSubText("");
        SetProgress(progress);
    }

    public void HideLoading()
    {
        Visible = false;
    }

    public void SetText(string text)
    {
        _label.Text = text;
    }

    public void SetSubText(string text)
    {
        _subLabel.Text = text;
    }

    public void SetProgress(float value)
    {
        _progressBar.Value = value;
    }
}