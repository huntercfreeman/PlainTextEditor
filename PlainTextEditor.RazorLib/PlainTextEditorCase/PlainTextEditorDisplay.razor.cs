using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
    private ElementReference _inputFocusTrap;

    private string PlainTextEditorDisplayId => $"rte_plain-text-editor-display_{PlainTextEditorKey.Guid}";
    private string InputFocusTrapId => $"rte_focus-trap_{PlainTextEditorKey.Guid}";
    private string ActiveRowId => $"rte_active-row_{PlainTextEditorKey.Guid}";

    private string IsFocusedCssClass => _isFocused
        ? "rte_focused"
        : "";
    
    private string InputFocusTrapTopStyleCss => $"top: calc({PlainTextEditorSelector.Value!.CurrentRowIndex + 1}em + {PlainTextEditorSelector.Value!.CurrentRowIndex * 8.6767}px - 25px)";

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
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
           JsRuntime.InvokeVoidAsync("plainTextEditor.subscribeScrollIntoView",
                InputFocusTrapId,
                PlainTextEditorKey.Guid);
        }

        JsRuntime.InvokeVoidAsync("plainTextEditor.scrollIntoViewIfOutOfViewport",
            _inputFocusTrap);

        base.OnAfterRender(firstRender);
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

    private void OnKeyDown(KeyboardEventArgs e)
    {
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

    private void FocusInputFocusTrapOnClick()
    {
        _previousSequenceKey = null;
        _inputFocusTrap.FocusAsync();
    }
    
    private async Task OnScroll(EventArgs e)
    {
        var scrollTop = await JsRuntime.InvokeAsync<double>("plainTextEditor.getScrollTop",
            PlainTextEditorDisplayId);

        Console.WriteLine($"scrollTop: {scrollTop}");
    }

    private string GetStyleCss()
    {
        return $"font-size: {PlainTextEditorSelector.Value?.RichTextEditorOptions.FontSizeInPixels ?? 0}px;";
    }

    protected override void Dispose(bool disposing)
    {
        PlainTextEditorSelector.SelectedValueChanged -= PlainTextEditorSelectorOnSelectedValueChanged;

        JsRuntime.InvokeVoidAsync("plainTextEditor.disposeScrollIntoView",
            InputFocusTrapId);

        base.Dispose(disposing);
    }
}
