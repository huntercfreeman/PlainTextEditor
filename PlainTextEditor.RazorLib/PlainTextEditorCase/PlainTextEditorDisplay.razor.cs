using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using PlainTextEditor.ClassLib.Keyboard;
using PlainTextEditor.ClassLib.Sequence;
using PlainTextEditor.ClassLib.Store.KeyDownEventCase;
using PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

namespace PlainTextEditor.RazorLib.PlainTextEditorCase;

public partial class PlainTextEditorDisplay : FluxorComponent, IDisposable
{
    [Inject]
    private IStateSelection<PlainTextEditorStates, IPlainTextEditor?> PlainTextEditorSelector { get; set; } = null!;
    [Inject]
    private IDispatcher Dispatcher { get; set; } = null!;
    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter, EditorRequired]
    public PlainTextEditorKey PlainTextEditorKey { get; set; } = null!;

    private bool _isFocused;
    private ElementReference _plainTextEditor;
    private Virtualize<(int Index, IPlainTextEditorRow PlainTextEditorRow)> _rowVirtualizeComponent = null!;
    private int _hadOnKeyDownEventCounter;

    private string PlainTextEditorDisplayId => $"rte_plain-text-editor-display_{PlainTextEditorKey.Guid}";
    private string ActiveRowPositionMarker => $"rte_focus-trap_{PlainTextEditorKey.Guid}";
    private string ActiveRowId => $"rte_active-row_{PlainTextEditorKey.Guid}";

    private string IsFocusedCssClass => _isFocused
        ? "rte_focused"
        : "";

    private string InputFocusTrapTopStyleCss => $"top: calc({PlainTextEditorSelector.Value!.CurrentRowIndex * 28.4}px - 12px);";

    private SequenceKey? _previousSequenceKey;

    protected override void OnInitialized()
    {
        PlainTextEditorSelector.Select(x => 
        {
            x.Map.TryGetValue(PlainTextEditorKey, out var value);
            return value;
        });

        PlainTextEditorSelector.SelectedValueChanged += PlainTextEditorSelectorOnSelectedValueChanged;

        base.OnInitialized();
    }

    private async void PlainTextEditorSelectorOnSelectedValueChanged(object? sender, IPlainTextEditor? e)
    {
        await _rowVirtualizeComponent.RefreshDataAsync();

        await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
           JsRuntime.InvokeVoidAsync("plainTextEditor.subscribeScrollIntoView",
                ActiveRowPositionMarker,
                PlainTextEditorKey.Guid);
        }

        if (_hadOnKeyDownEventCounter > 0)
        {
            _hadOnKeyDownEventCounter = 0;

            JsRuntime.InvokeVoidAsync("plainTextEditor.scrollIntoViewIfOutOfViewport",
                ActiveRowPositionMarker);
        }

        base.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// @onkeydown by default takes an EventCallback which causes
    /// many redundant StateHasChanged calls.
    /// 
    /// Fluxor IStateSelection correctly does not render in certain conditions but
    /// the EventCallback implicitely calling StateHasChanged results in ShouldRender being necessary
    /// </summary>
    /// <returns></returns>
    protected override bool ShouldRender()
    {
        if (PlainTextEditorSelector.Value is null)
            return true;

        var shouldRender = false;

        if(PlainTextEditorSelector.Value.SequenceKey != _previousSequenceKey)
            shouldRender = true;

        _previousSequenceKey = PlainTextEditorSelector.Value.SequenceKey;

        return shouldRender;
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        _hadOnKeyDownEventCounter++;

        Dispatcher.Dispatch(
            new KeyDownEventAction(PlainTextEditorKey,
                new ClassLib.Keyboard.KeyDownEventRecord(
                    e.Key,
                    e.Code,
                    e.CtrlKey,
                    e.ShiftKey,
                    e.AltKey
                )
            )
        );
    }
    
    private void OnFocusIn()
    {
        _previousSequenceKey = null;
        _isFocused = true;
    }

    private void OnFocusOut()
    {
        _previousSequenceKey = null;
        _isFocused = false;
    }

    private void FocusPlainTextEditorOnClick()
    {
        _previousSequenceKey = null;
        _plainTextEditor.FocusAsync();
    }

    private string GetStyleCss()
    {
        return $"font-size: {PlainTextEditorSelector.Value?.RichTextEditorOptions.FontSizeInPixels ?? 0}px;";
    }

    private ValueTask<ItemsProviderResult<(int Index, IPlainTextEditorRow PlainTextEditorRow)>> RowItemsProvider(
        ItemsProviderRequest request)
    {
        var currentPlainTextEditor = PlainTextEditorSelector.Value;

        (int Index, IPlainTextEditorRow PlainTextEditorRow)[] rowTuples =
            Array.Empty<(int Index, IPlainTextEditorRow PlainTextEditorRow)>();

        if (currentPlainTextEditor is null)
            return ValueTask.FromResult(new ItemsProviderResult<(int Index, IPlainTextEditorRow PlainTextEditorRow)>(rowTuples,
                0));

        var numberOfRows = Math.Min(request.Count, currentPlainTextEditor.List.Count - request.StartIndex);

        if (numberOfRows > 0)
        {
            rowTuples = currentPlainTextEditor.List
                .Select((row, index) => (index, row))
                .Skip(request.StartIndex)
                .Take(numberOfRows)
                .ToArray();
        }

        return ValueTask.FromResult(new ItemsProviderResult<(int Index, IPlainTextEditorRow PlainTextEditorRow)>(rowTuples,
                currentPlainTextEditor.List.Count));
    }

    protected override void Dispose(bool disposing)
    {
        PlainTextEditorSelector.SelectedValueChanged -= PlainTextEditorSelectorOnSelectedValueChanged;

        JsRuntime.InvokeVoidAsync("plainTextEditor.disposeScrollIntoView",
            ActiveRowPositionMarker);

        base.Dispose(disposing);
    }
}
