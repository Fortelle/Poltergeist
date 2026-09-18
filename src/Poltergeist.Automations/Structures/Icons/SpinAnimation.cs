namespace Poltergeist.Automations.Structures;

public class SpinAnimation : IconAnimation
{
    public int Count { get; set; }
    public bool Anticlockwise { get; set; }
    public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);
}
