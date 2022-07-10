namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public record RichTextEditorRowKey(Guid Guid)
{
    public static RichTextEditorRowKey NewRichTextEditorRowKey()
    {
        return new RichTextEditorRowKey(Guid.NewGuid());
    }
}
