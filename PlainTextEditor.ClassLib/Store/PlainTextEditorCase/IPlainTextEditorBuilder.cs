namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public interface IPlainTextEditorBuilder
{
    public IPlainTextEditorBuilder Add(IPlainTextEditorRow row);
    public IPlainTextEditorBuilder Insert(int index, IPlainTextEditorRow row);
    public IPlainTextEditorBuilder Remove(PlainTextEditorRowKey plainTextEditorRowKey);
    public IPlainTextEditorBuilder CurrentRowIndexOf(int currentRowIndex);
    public IPlainTextEditorBuilder CurrentTokenIndexOf(int currentTokenIndex);
    public IPlainTextEditor Build();
}