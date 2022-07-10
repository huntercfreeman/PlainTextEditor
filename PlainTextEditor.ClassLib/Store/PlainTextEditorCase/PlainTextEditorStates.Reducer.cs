using Fluxor;
using PlainTextEditor.ClassLib.Sequence;
using PlainTextEditor.ClassLib.Store.KeyDownEventCase;
using System.Collections.Immutable;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    public class PlainTextEditorStatesReducer
    {
        [ReducerMethod]
        public static PlainTextEditorStates ReduceConstructPlainTextEditorAction(PlainTextEditorStates previousPlainTextEditorStates,
            ConstructPlainTextEditorRecordAction constructPlainTextEditorRecordAction)
        {
            var nextPlainTextEditorMap = new Dictionary<PlainTextEditorKey, IPlainTextEditor>(previousPlainTextEditorStates.Map);
            var nextPlainTextEditorList = new List<PlainTextEditorKey>(previousPlainTextEditorStates.Array);

            var plainTextEditor = new 
                PlainTextEditorRecord(constructPlainTextEditorRecordAction.PlainTextEditorKey);

            nextPlainTextEditorMap[constructPlainTextEditorRecordAction.PlainTextEditorKey] = plainTextEditor;
            nextPlainTextEditorList.Add(constructPlainTextEditorRecordAction.PlainTextEditorKey);

            return new PlainTextEditorStates(nextPlainTextEditorMap.ToImmutableDictionary(), nextPlainTextEditorList.ToImmutableArray());
        }
        
        [ReducerMethod]
        public static PlainTextEditorStates ReduceDeconstructPlainTextEditorRecordAction(PlainTextEditorStates previousPlainTextEditorStates,
            DeconstructPlainTextEditorRecordAction deconstructPlainTextEditorRecordAction)
        {
            var nextPlainTextEditorMap = new Dictionary<PlainTextEditorKey, IPlainTextEditor>(previousPlainTextEditorStates.Map);
            var nextPlainTextEditorList = new List<PlainTextEditorKey>(previousPlainTextEditorStates.Array);

            nextPlainTextEditorMap.Remove(deconstructPlainTextEditorRecordAction.PlainTextEditorKey);
            nextPlainTextEditorList.Remove(deconstructPlainTextEditorRecordAction.PlainTextEditorKey);

            return new PlainTextEditorStates(nextPlainTextEditorMap.ToImmutableDictionary(), nextPlainTextEditorList.ToImmutableArray());
        }
        
        [ReducerMethod]
        public static PlainTextEditorStates ReduceKeyDownEventAction(PlainTextEditorStates previousPlainTextEditorStates,
            KeyDownEventAction keyDownEventAction)
        {
#if DEBUG
            var startTimeUtc = DateTime.UtcNow;
#endif
            var nextPlainTextEditorMap = new Dictionary<PlainTextEditorKey, IPlainTextEditor>(previousPlainTextEditorStates.Map);
            var nextPlainTextEditorList = new List<PlainTextEditorKey>(previousPlainTextEditorStates.Array);
            
            var focusedPlainTextEditor = previousPlainTextEditorStates.Map[keyDownEventAction.FocusedPlainTextEditorKey]
                as PlainTextEditorRecord;

            if (focusedPlainTextEditor is null) 
                return previousPlainTextEditorStates;

            var replacementPlainTextEditor = PlainTextEditorStates.StateMachine
                .HandleKeyDownEvent(focusedPlainTextEditor, keyDownEventAction.KeyDownEventRecord) with
            {
                SequenceKey = SequenceKey.NewSequenceKey()
            };

            nextPlainTextEditorMap[keyDownEventAction.FocusedPlainTextEditorKey] = replacementPlainTextEditor;

            var nextImmutableMap = nextPlainTextEditorMap.ToImmutableDictionary();
            var nextImmutableArray = nextPlainTextEditorList.ToImmutableArray();
            
#if DEBUG
            var endTimeUtc = DateTime.UtcNow;

            var elapsedTimeSpan = endTimeUtc - startTimeUtc;

            Console.WriteLine($"Miliseconds: {elapsedTimeSpan.TotalMilliseconds}");
#endif
            
            return new PlainTextEditorStates(nextImmutableMap, nextImmutableArray);
        }
        
        [ReducerMethod]
        public static PlainTextEditorStates ReducePlainTextEditorOnClickAction(PlainTextEditorStates previousPlainTextEditorStates,
            PlainTextEditorOnClickAction plainTextEditorOnClickAction)
        {
            var nextPlainTextEditorMap = new Dictionary<PlainTextEditorKey, IPlainTextEditor>(previousPlainTextEditorStates.Map);
            var nextPlainTextEditorList = new List<PlainTextEditorKey>(previousPlainTextEditorStates.Array);
            
            var focusedPlainTextEditor = previousPlainTextEditorStates.Map[plainTextEditorOnClickAction.FocusedPlainTextEditorKey]
                as PlainTextEditorRecord;

            if (focusedPlainTextEditor is null) 
                return previousPlainTextEditorStates;

            var replacementPlainTextEditor = PlainTextEditorStates.StateMachine
                .HandleOnClickEvent(focusedPlainTextEditor, plainTextEditorOnClickAction) with
            {
                SequenceKey = SequenceKey.NewSequenceKey()
            };

            nextPlainTextEditorMap[plainTextEditorOnClickAction.FocusedPlainTextEditorKey] = replacementPlainTextEditor;

            return new PlainTextEditorStates(nextPlainTextEditorMap.ToImmutableDictionary(), nextPlainTextEditorList.ToImmutableArray());
        }
    }
}

