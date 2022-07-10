using RichTextEditor.ClassLib.Sequence;
using System.Collections.Immutable;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public interface IRichTextEditor
{
    public RichTextEditorKey RichTextEditorKey { get; } 
    public SequenceKey SequenceKey { get; } 
    public ImmutableDictionary<RichTextEditorRowKey, IRichTextEditorRow> Map { get; } 
    public ImmutableArray<RichTextEditorRowKey> Array { get; }
    public int CurrentRowIndex { get; }
    public int CurrentTokenIndex { get; }
}
