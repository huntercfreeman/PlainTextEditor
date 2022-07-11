using PlainTextEditor.ClassLib.Sequence;
using System.Collections.Immutable;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public interface IPlainTextEditor
{
    public PlainTextEditorKey PlainTextEditorKey { get; } 
    public SequenceKey SequenceKey { get; } 
    public ImmutableDictionary<PlainTextEditorRowKey, IPlainTextEditorRow> Map { get; } 
    public ImmutableArray<PlainTextEditorRowKey> Array { get; }
    public int CurrentRowIndex { get; }
    public int CurrentTokenIndex { get; }
    public RichTextEditorOptions RichTextEditorOptions { get; }
}
