namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public interface IRichTextEditorBuilder
{
    public IRichTextEditorBuilder Add(IRichTextEditorRow row);
    public IRichTextEditorBuilder Insert(int index, IRichTextEditorRow row);
    public IRichTextEditorBuilder Remove(RichTextEditorRowKey richTextEditorRowKey);
    public IRichTextEditorBuilder CurrentRowIndexOf(int currentRowIndex);
    public IRichTextEditorBuilder CurrentTokenIndexOf(int currentTokenIndex);
    public IRichTextEditor Build();
}