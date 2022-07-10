using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

[FeatureState]
public partial record RichTextEditorStates(ImmutableDictionary<RichTextEditorKey, IRichTextEditor> Map, 
    ImmutableArray<RichTextEditorKey> Array)
{
    private RichTextEditorStates() : this(new Dictionary<RichTextEditorKey, IRichTextEditor>().ToImmutableDictionary(),
        new RichTextEditorKey[0].ToImmutableArray())
    {
        
    }
}
