using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using RichTextEditor.ClassLib.Store.RichTextEditorCase;
using RichTextEditor.ClassLib.WebAssemblyFix;

namespace RichTextEditor.ClassLib.Services;

public class RichTextEditorService : IRichTextEditorService, IDisposable
{
    private readonly IDispatcher _dispatcher;
    private readonly IState<RichTextEditorStates> _richTextEditorStatesWrap;

    private readonly SemaphoreSlim _onRichTextEditorConstructedActionsSemaphoreSlim = new(1, 1);
    private readonly Dictionary<RichTextEditorKey, Func<Task>> _onRichTextEditorConstructedActionMap = new();

    public RichTextEditorService(IDispatcher dispatcher, IState<RichTextEditorStates> richTextEditorStatesWrap)
    {
        _dispatcher = dispatcher;
        _richTextEditorStatesWrap = richTextEditorStatesWrap;

        _richTextEditorStatesWrap.StateChanged += OnRichTextEditorStatesWrapStateChanged;
    }

    private async void OnRichTextEditorStatesWrapStateChanged(object? sender, EventArgs e)
    {
        try
        {
            await _onRichTextEditorConstructedActionsSemaphoreSlim.WaitAsync();

            var onRichTextEditorConstructedActions = _onRichTextEditorConstructedActionMap.AsEnumerable();

            foreach (var pair in onRichTextEditorConstructedActions)
            {
                if (_richTextEditorStatesWrap.Value.Map.ContainsKey(pair.Key)) 
                {
                    await pair.Value.Invoke();
                    _onRichTextEditorConstructedActionMap.Remove(pair.Key);
                }
            }
        }
        finally
        {
            _onRichTextEditorConstructedActionsSemaphoreSlim.Release();
        }
    }

    public async Task ConstructRichTextEditorAsync(RichTextEditorKey richTextEditorKey, Func<Task> richTextEditorWasConstructedCallback)
    {
        try
        {
            await _onRichTextEditorConstructedActionsSemaphoreSlim.WaitAsync();
            _onRichTextEditorConstructedActionMap.Add(richTextEditorKey, richTextEditorWasConstructedCallback);
        }
        finally
        {
            _onRichTextEditorConstructedActionsSemaphoreSlim.Release();
        }

        _dispatcher.Dispatch(
            new WebAssemblyFixDelayAction(
                new ConstructRichTextEditorRecordAction(richTextEditorKey)));
    }
    
    public void DeconstructRichTextEditor(RichTextEditorKey richTextEditorKey)
    {
        _dispatcher.Dispatch(
            new WebAssemblyFixDelayAction(
                new DeconstructRichTextEditorRecordAction(richTextEditorKey)));
    }

    public void Dispose()
    {
        _richTextEditorStatesWrap.StateChanged -= OnRichTextEditorStatesWrapStateChanged;
    }
}
