namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public interface ITextToken
{
    public TextTokenKey Key { get; }
    public abstract string PlainText { get; }
    public TextTokenKind Kind { get; }
    public int? IndexInPlainText { get; }
}
