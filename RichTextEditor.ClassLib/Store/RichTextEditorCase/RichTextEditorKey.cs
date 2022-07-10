namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public record RichTextEditorKey(Guid Guid)
{
    public static RichTextEditorKey NewRichTextEditorKey()
    {
        return new RichTextEditorKey(Guid.NewGuid());
    }
}
