namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public record TextTokenKey(Guid Guid)
{
    public static TextTokenKey NewTextTokenKey()
    {
        return new TextTokenKey(Guid.NewGuid());
    }
}
