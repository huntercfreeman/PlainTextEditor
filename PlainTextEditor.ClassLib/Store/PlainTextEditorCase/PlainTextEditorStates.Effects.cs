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
        private int _counter = 0;

        [EffectMethod]
        public async Task HandleWebAssemblyFixDelayAction(WebAssemblyFixDelayAction webAssemblyFixDelayAction,
            IDispatcher dispatcher)
        {
            Console.WriteLine($"HandleWebAssemblyFixDelayAction {_counter++}");
            await Task.Delay(1);

            dispatcher.Dispatch(webAssemblyFixDelayAction.Action);
        }
    }
}

