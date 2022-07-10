using Fluxor;
using RichTextEditor.ClassLib.Sequence;
using RichTextEditor.ClassLib.Store.KeyDownEventCase;
using System.Collections.Immutable;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    public class RichTextEditorStatesReducer
    {
        [ReducerMethod]
        public static RichTextEditorStates ReduceConstructRichTextEditorAction(RichTextEditorStates previousRichTextEditorStates,
            ConstructRichTextEditorRecordAction constructRichTextEditorRecordAction)
        {
            var nextRichTextEditorMap = new Dictionary<RichTextEditorKey, IRichTextEditor>(previousRichTextEditorStates.Map);
            var nextRichTextEditorList = new List<RichTextEditorKey>(previousRichTextEditorStates.Array);

            var richTextEditor = new 
                RichTextEditorRecord(constructRichTextEditorRecordAction.RichTextEditorKey);

            nextRichTextEditorMap[constructRichTextEditorRecordAction.RichTextEditorKey] = richTextEditor;
            nextRichTextEditorList.Add(constructRichTextEditorRecordAction.RichTextEditorKey);

            return new RichTextEditorStates(nextRichTextEditorMap.ToImmutableDictionary(), nextRichTextEditorList.ToImmutableArray());
        }
        
        [ReducerMethod]
        public static RichTextEditorStates ReduceDeconstructRichTextEditorRecordAction(RichTextEditorStates previousRichTextEditorStates,
            DeconstructRichTextEditorRecordAction deconstructRichTextEditorRecordAction)
        {
            var nextRichTextEditorMap = new Dictionary<RichTextEditorKey, IRichTextEditor>(previousRichTextEditorStates.Map);
            var nextRichTextEditorList = new List<RichTextEditorKey>(previousRichTextEditorStates.Array);

            nextRichTextEditorMap.Remove(deconstructRichTextEditorRecordAction.RichTextEditorKey);
            nextRichTextEditorList.Remove(deconstructRichTextEditorRecordAction.RichTextEditorKey);

            return new RichTextEditorStates(nextRichTextEditorMap.ToImmutableDictionary(), nextRichTextEditorList.ToImmutableArray());
        }
        
        [ReducerMethod]
        public static RichTextEditorStates ReduceKeyDownEventAction(RichTextEditorStates previousRichTextEditorStates,
            KeyDownEventAction keyDownEventAction)
        {
            Console.WriteLine($"ReduceKeyDownEventAction {keyDownEventAction.KeyDownEventRecord.Key ?? string.Empty}");
            var nextRichTextEditorMap = new Dictionary<RichTextEditorKey, IRichTextEditor>(previousRichTextEditorStates.Map);
            var nextRichTextEditorList = new List<RichTextEditorKey>(previousRichTextEditorStates.Array);
            
            var focusedRichTextEditor = previousRichTextEditorStates.Map[keyDownEventAction.FocusedRichTextEditorKey]
                as RichTextEditorRecord;

            if (focusedRichTextEditor is null) 
                return previousRichTextEditorStates;

            var replacementRichTextEditor = RichTextEditorStates.StateMachine
                .HandleKeyDownEvent(focusedRichTextEditor, keyDownEventAction.KeyDownEventRecord) with
            {
                SequenceKey = SequenceKey.NewSequenceKey()
            };

            nextRichTextEditorMap[keyDownEventAction.FocusedRichTextEditorKey] = replacementRichTextEditor;

            return new RichTextEditorStates(nextRichTextEditorMap.ToImmutableDictionary(), nextRichTextEditorList.ToImmutableArray());
        }
        
        [ReducerMethod]
        public static RichTextEditorStates ReduceRichTextEditorOnClickAction(RichTextEditorStates previousRichTextEditorStates,
            RichTextEditorOnClickAction richTextEditorOnClickAction)
        {
            var nextRichTextEditorMap = new Dictionary<RichTextEditorKey, IRichTextEditor>(previousRichTextEditorStates.Map);
            var nextRichTextEditorList = new List<RichTextEditorKey>(previousRichTextEditorStates.Array);
            
            var focusedRichTextEditor = previousRichTextEditorStates.Map[richTextEditorOnClickAction.FocusedRichTextEditorKey]
                as RichTextEditorRecord;

            if (focusedRichTextEditor is null) 
                return previousRichTextEditorStates;

            var replacementRichTextEditor = RichTextEditorStates.StateMachine
                .HandleOnClickEvent(focusedRichTextEditor, richTextEditorOnClickAction) with
            {
                SequenceKey = SequenceKey.NewSequenceKey()
            };

            nextRichTextEditorMap[richTextEditorOnClickAction.FocusedRichTextEditorKey] = replacementRichTextEditor;

            return new RichTextEditorStates(nextRichTextEditorMap.ToImmutableDictionary(), nextRichTextEditorList.ToImmutableArray());
        }
    }
}

