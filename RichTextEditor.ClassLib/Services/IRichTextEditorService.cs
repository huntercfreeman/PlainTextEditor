using RichTextEditor.ClassLib.Store.RichTextEditorCase;

namespace RichTextEditor.ClassLib.Services;

public interface IRichTextEditorService
{
    public Task ConstructRichTextEditorAsync(RichTextEditorKey richTextEditorKey, Func<Task> richTextEditorWasConstructedCallback);
    public void DeconstructRichTextEditor(RichTextEditorKey richTextEditorKey);
}
