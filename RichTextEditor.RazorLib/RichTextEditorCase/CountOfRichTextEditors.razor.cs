using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;

namespace RichTextEditor.RazorLib.RichTextEditorCase;

public partial class CountOfRichTextEditors : FluxorComponent
{
    [Inject]
    private IState<RichTextEditorStates> RichTextEditorStatesWrap { get; set; } = null!;
}
