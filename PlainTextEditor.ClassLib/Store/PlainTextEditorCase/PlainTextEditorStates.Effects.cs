using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using PlainTextEditor.ClassLib.Keyboard;
using PlainTextEditor.ClassLib.Store.KeyDownEventCase;
using PlainTextEditor.ClassLib.WebAssemblyFix;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    public class PlainTextEditorStatesEffects
    {
        // TODO: Remove this
        [EffectMethod]
        public Task HandleWebAssemblyFixDelayAction(WebAssemblyFixDelayAction webAssemblyFixDelayAction,
            IDispatcher dispatcher)
        {
            dispatcher.Dispatch(webAssemblyFixDelayAction.Action);
            
            return Task.CompletedTask;
        }
    }
}

