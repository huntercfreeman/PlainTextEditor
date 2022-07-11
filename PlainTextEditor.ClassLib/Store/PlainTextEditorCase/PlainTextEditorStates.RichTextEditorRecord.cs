using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using PlainTextEditor.ClassLib.Sequence;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    private record PlainTextEditorRecord(PlainTextEditorKey PlainTextEditorKey,
        SequenceKey SequenceKey,
        ImmutableDictionary<PlainTextEditorRowKey, IPlainTextEditorRow> Map, 
        ImmutableArray<PlainTextEditorRowKey> Array,
        int CurrentRowIndex,
        int CurrentTokenIndex,
        RichTextEditorOptions RichTextEditorOptions)
            : IPlainTextEditor
    {
        public PlainTextEditorRecord(PlainTextEditorKey plainTextEditorKey) : this(plainTextEditorKey,
            SequenceKey.NewSequenceKey(),
            new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>().ToImmutableDictionary(),
            new PlainTextEditorRowKey[0].ToImmutableArray(),
            CurrentRowIndex: 0,
            CurrentTokenIndex: 0,
            new RichTextEditorOptions())
        {
            var startingRow = new PlainTextEditorRow();

            Map = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>
            {
                { 
                    startingRow.Key,
                    startingRow 
                }
            }.ToImmutableDictionary();

            Array = new PlainTextEditorRowKey[]
            {
                startingRow.Key
            }.ToImmutableArray();
        }

        public PlainTextEditorRowKey CurrentPlainTextEditorRowKey => Array[CurrentRowIndex];
        public IPlainTextEditorRow CurrentPlainTextEditorRow => Map[CurrentPlainTextEditorRowKey];
        private PlainTextEditorRow StateMachineCurrentPlainTextEditorRow => Map[CurrentPlainTextEditorRowKey]
            as PlainTextEditorRow
            ?? throw new ApplicationException($"Expected {nameof(PlainTextEditorRow)}");
        
        public TextTokenKey CurrentTextTokenKey => CurrentPlainTextEditorRow.Array[CurrentTokenIndex];
        public ITextToken CurrentTextToken => CurrentPlainTextEditorRow.Map[CurrentTextTokenKey];

        public T GetCurrentTextTokenAs<T>()
            where T : class
        {
            return CurrentTextToken as T
                ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }
        
        public T GetCurrentPlainTextEditorRowAs<T>()
            where T : class
        {
            return CurrentPlainTextEditorRow as T
                ?? throw new ApplicationException($"Expected {typeof(T).Name}");
        }

        public IPlainTextEditorBuilder With()
    {
        return new PlainTextEditorBuilder(this);
    }
        
    private class PlainTextEditorBuilder : IPlainTextEditorBuilder
    {
        public PlainTextEditorBuilder()
        {
            
        }

        public PlainTextEditorBuilder(IPlainTextEditor plainTextEditor)
        {
            Key = plainTextEditor.PlainTextEditorKey;
            Map = new(plainTextEditor.Map);
            List = new(plainTextEditor.Array);
            CurrentRowIndex = plainTextEditor.CurrentRowIndex;
            CurrentTokenIndex = plainTextEditor.CurrentTokenIndex;
        }
        
        private PlainTextEditorKey Key { get; } = PlainTextEditorKey.NewPlainTextEditorKey();
        private Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow> Map { get; } = new();  
        private List<PlainTextEditorRowKey> List { get; } = new();
        private int CurrentRowIndex { get; set; }
        private int CurrentTokenIndex { get; set; }

        public IPlainTextEditorBuilder Add(IPlainTextEditorRow plainTextEditorRow)
        {
            Map.Add(plainTextEditorRow.Key, plainTextEditorRow);
            List.Add(plainTextEditorRow.Key);

            return this;
        }
        
        public IPlainTextEditorBuilder Insert(int index, IPlainTextEditorRow plainTextEditorRow)
        {
            Map.Add(plainTextEditorRow.Key, plainTextEditorRow);
            List.Insert(index, plainTextEditorRow.Key);

            return this;
        }

        public IPlainTextEditorBuilder Remove(PlainTextEditorRowKey plainTextEditorRowKey)
        {
            Map.Remove(plainTextEditorRowKey);
            List.Remove(plainTextEditorRowKey);

            return this;
        }
        
        public IPlainTextEditorBuilder CurrentRowIndexOf(int currentRowIndex)
        {
            CurrentRowIndex = currentRowIndex;

            return this;
        }
        
        public IPlainTextEditorBuilder CurrentTokenIndexOf(int currentTokenIndex)
        {
            CurrentTokenIndex = currentTokenIndex;

            return this;
        }
        
        public IPlainTextEditor Build()
        {
            return new PlainTextEditorRecord(Key,
                SequenceKey.NewSequenceKey(),
                Map.ToImmutableDictionary(),
                List.ToImmutableArray(),
                CurrentRowIndex,
                CurrentTokenIndex,
                new RichTextEditorOptions());
        }
    }
    }
}
