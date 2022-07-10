using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using RichTextEditor.ClassLib.Sequence;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    private record RichTextEditorRecord(RichTextEditorKey RichTextEditorKey,
        SequenceKey SequenceKey,
        ImmutableDictionary<RichTextEditorRowKey, IRichTextEditorRow> Map, 
        ImmutableArray<RichTextEditorRowKey> Array,
        int CurrentRowIndex,
        int CurrentTokenIndex)
            : IRichTextEditor
    {
        public RichTextEditorRecord(RichTextEditorKey richTextEditorKey) : this(richTextEditorKey,
            SequenceKey.NewSequenceKey(),
            new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>().ToImmutableDictionary(),
            new RichTextEditorRowKey[0].ToImmutableArray(),
            CurrentRowIndex: 0,
            CurrentTokenIndex: 0)
        {
            var startingRow = new RichTextEditorRow();

            Map = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>
            {
                { 
                    startingRow.Key,
                    startingRow 
                }
            }.ToImmutableDictionary();

            Array = new RichTextEditorRowKey[]
            {
                startingRow.Key
            }.ToImmutableArray();
        }

        public RichTextEditorRowKey CurrentRichTextEditorRowKey => Array[CurrentRowIndex];
        public IRichTextEditorRow CurrentRichTextEditorRow => Map[CurrentRichTextEditorRowKey];
        private RichTextEditorRow StateMachineCurrentRichTextEditorRow => Map[CurrentRichTextEditorRowKey]
            as RichTextEditorRow
            ?? throw new ApplicationException($"Expected {nameof(RichTextEditorRow)}");
        
        public TextTokenKey CurrentTextTokenKey => CurrentRichTextEditorRow.Array[CurrentTokenIndex];
        public ITextToken CurrentTextToken => CurrentRichTextEditorRow.Map[CurrentTextTokenKey];

        public T GetCurrentTextTokenAs<T>()
            where T : class
        {
            return CurrentTextToken as T
                ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }
        
        public T GetCurrentRichTextEditorRowAs<T>()
            where T : class
        {
            return CurrentRichTextEditorRow as T
                ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }

        public IRichTextEditorBuilder With()
    {
        return new RichTextEditorBuilder(this);
    }
        
    private class RichTextEditorBuilder : IRichTextEditorBuilder
    {
        public RichTextEditorBuilder()
        {
            
        }

        public RichTextEditorBuilder(IRichTextEditor richTextEditor)
        {
            Key = richTextEditor.RichTextEditorKey;
            Map = new(richTextEditor.Map);
            List = new(richTextEditor.Array);
            CurrentRowIndex = richTextEditor.CurrentRowIndex;
            CurrentTokenIndex = richTextEditor.CurrentTokenIndex;
        }
        
        private RichTextEditorKey Key { get; } = RichTextEditorKey.NewRichTextEditorKey();
        private Dictionary<RichTextEditorRowKey, IRichTextEditorRow> Map { get; } = new();  
        private List<RichTextEditorRowKey> List { get; } = new();
        private int CurrentRowIndex { get; set; }
        private int CurrentTokenIndex { get; set; }

        public IRichTextEditorBuilder Add(IRichTextEditorRow richTextEditorRow)
        {
            Map.Add(richTextEditorRow.Key, richTextEditorRow);
            List.Add(richTextEditorRow.Key);

            return this;
        }
        
        public IRichTextEditorBuilder Insert(int index, IRichTextEditorRow richTextEditorRow)
        {
            Map.Add(richTextEditorRow.Key, richTextEditorRow);
            List.Insert(index, richTextEditorRow.Key);

            return this;
        }

        public IRichTextEditorBuilder Remove(RichTextEditorRowKey richTextEditorRowKey)
        {
            Map.Remove(richTextEditorRowKey);
            List.Remove(richTextEditorRowKey);

            return this;
        }
        
        public IRichTextEditorBuilder CurrentRowIndexOf(int currentRowIndex)
        {
            CurrentRowIndex = currentRowIndex;

            return this;
        }
        
        public IRichTextEditorBuilder CurrentTokenIndexOf(int currentTokenIndex)
        {
            CurrentTokenIndex = currentTokenIndex;

            return this;
        }
        
        public IRichTextEditor Build()
        {
            return new RichTextEditorRecord(Key,
                SequenceKey.NewSequenceKey(),
                Map.ToImmutableDictionary(),
                List.ToImmutableArray(),
                CurrentRowIndex,
                CurrentTokenIndex);
        }
    }
    }
}
