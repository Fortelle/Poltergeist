namespace Poltergeist.Automations.Structures;

public class EmojiIcon : IconInfo
{
    public string Emoji { get; }

    public EmojiIcon(string emoji)
    {
        Emoji = emoji;
    }

    public override string ToString() => Emoji;
}
