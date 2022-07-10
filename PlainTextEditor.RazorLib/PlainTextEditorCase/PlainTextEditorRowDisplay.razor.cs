using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using PlainTextEditor.ClassLib.Sequence;
using PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

namespace PlainTextEditor.RazorLib.PlainTextEditorCase;

public partial class PlainTextEditorRowDisplay : FluxorComponent
{
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    
    [CascadingParameter(Name="CurrentRowIndex")]
    public int PlainTextEditorCurrentRowIndex { get; set; }
    [CascadingParameter(Name="ActiveRowId")]
    public string ActiveRowId { get; set; } = null!;
    [CascadingParameter(Name="RowIndex")]
    public int RowIndex { get; set; }
    [CascadingParameter]
    public PlainTextEditorKey PlainTextEditorKey { get; set; } = null!;

    [Parameter, EditorRequired]
    public IPlainTextEditorRow PlainTextEditorRow { get; set; } = null!;
    [Parameter, EditorRequired]
    public int MostDigitsInARowNumber { get; set; }

    private bool _characterWasClicked;
    private SequenceKey? _previousSequenceKey;

    private string IsActiveCss => PlainTextEditorCurrentRowIndex == RowIndex
        ? "rte_active"
        : string.Empty;

    private string WidthStyleCss => $"width: calc(100% - {MostDigitsInARowNumber}ch);";
    
    private string IsActiveRowId => PlainTextEditorCurrentRowIndex == RowIndex
        ? ActiveRowId
        : string.Empty;

    protected override bool ShouldRender()
    {
        var shouldRender = false;

        if (PlainTextEditorRow.SequenceKey != _previousSequenceKey)
            shouldRender = true;

        _previousSequenceKey = PlainTextEditorRow.SequenceKey;

        return shouldRender;
    }

    private void DispatchPlainTextEditorOnClickAction()
    {
        if (!_characterWasClicked)
        {
            Dispatcher.Dispatch(
                new PlainTextEditorOnClickAction(
                    PlainTextEditorKey,
                    RowIndex,
                    PlainTextEditorRow.Array.Length - 1,
                    null
                )
            );
        }
        else
        {
            _characterWasClicked = false;
        }
    }
}
