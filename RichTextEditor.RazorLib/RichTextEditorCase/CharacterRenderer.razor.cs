using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;
using RichTextEditor.ClassLib.WebAssemblyFix;

namespace RichTextEditor.RazorLib.RichTextEditorCase;

public partial class CharacterRenderer : ComponentBase
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    
    [CascadingParameter]
    public bool IsFocused { get; set; }
    [CascadingParameter(Name="TokenIndex")]
    public int TokenIndex { get; set; }
    [CascadingParameter(Name="RowIndex")]
    public int RowIndex { get; set; }
    [CascadingParameter]
    public RichTextEditorKey RichTextEditorKey { get; set; } = null!;
    [CascadingParameter(Name="NotifyCharacterWasClicked")]
    public Action NotifyCharacterWasClicked { get; set; } = null!;
    
    [Parameter, EditorRequired]
    // The html escaped character for space is &nbsp; which
    // requires more than 1 character to represent therefore this is of type string
    public string Character { get; set; } = null!;
    [Parameter, EditorRequired]
    public int CharacterIndex { get; set; }
    [Parameter, EditorRequired]
    public bool ShouldDisplayCursor { get; set; }

    private void DispatchRichTextEditorOnClickAction()
    {
        NotifyCharacterWasClicked();

        Dispatcher.Dispatch(
            new WebAssemblyFixDelayAction(
                new RichTextEditorOnClickAction(
                    RichTextEditorKey,
                    RowIndex,
                    TokenIndex,
                    CharacterIndex
                )
            )
        );
    }
}
