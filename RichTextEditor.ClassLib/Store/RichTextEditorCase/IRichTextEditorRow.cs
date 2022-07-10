using RichTextEditor.ClassLib.Sequence;
using System.Collections.Immutable;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public interface IRichTextEditorRow
{
    public RichTextEditorRowKey Key { get; } 
    public SequenceKey SequenceKey { get; } 
    public ImmutableDictionary<TextTokenKey, ITextToken> Map { get; }
    public ImmutableArray<TextTokenKey> Array { get; }
    
    public IRichTextEditorRowBuilder With();
}
