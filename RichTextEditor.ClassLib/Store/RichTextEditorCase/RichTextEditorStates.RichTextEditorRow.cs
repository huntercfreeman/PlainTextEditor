using RichTextEditor.ClassLib.Sequence;
using System.Collections.Immutable;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    private record RichTextEditorRow(RichTextEditorRowKey Key, 
        SequenceKey SequenceKey,
        ImmutableDictionary<TextTokenKey, ITextToken> Map,
        ImmutableArray<TextTokenKey> Array)
            : IRichTextEditorRow
    {
        public RichTextEditorRow() : this(RichTextEditorRowKey.NewRichTextEditorRowKey(), 
            SequenceKey.NewSequenceKey(),
            new Dictionary<TextTokenKey, ITextToken>().ToImmutableDictionary(),
            new TextTokenKey[0].ToImmutableArray())
        {
            var startOfRowToken = new StartOfRowTextToken()
            {
                IndexInPlainText = 0
            };

            Map = new Dictionary<TextTokenKey, ITextToken>
            {
                { 
                    startOfRowToken.Key, 
                    startOfRowToken
                }
            }.ToImmutableDictionary();

            Array = new TextTokenKey[]
            {
                startOfRowToken.Key
            }.ToImmutableArray();
        }

        public IRichTextEditorRowBuilder With()
        {
            return new RichTextEditorRowBuilder(this);
        }
        
        private class RichTextEditorRowBuilder : IRichTextEditorRowBuilder
        {
            public RichTextEditorRowBuilder()
            {
                
            }

            public RichTextEditorRowBuilder(RichTextEditorRow richTextEditorRow)
            {
                Key = richTextEditorRow.Key;
                Map = new(richTextEditorRow.Map);
                List = new(richTextEditorRow.Array);
            }
            
            private RichTextEditorRowKey Key { get; } = RichTextEditorRowKey.NewRichTextEditorRowKey();
            private Dictionary<TextTokenKey, ITextToken> Map { get; } = new();  
            private List<TextTokenKey> List { get; } = new();

            public IRichTextEditorRowBuilder Add(ITextToken token)
            {
                Map.Add(token.Key, token);
                List.Add(token.Key);

                return this;
            }
            
            public IRichTextEditorRowBuilder Insert(int index, ITextToken token)
            {
                Map.Add(token.Key, token);
                List.Insert(index, token.Key);

                return this;
            }

            public IRichTextEditorRowBuilder Remove(TextTokenKey textTokenKey)
            {
                Map.Remove(textTokenKey);
                List.Remove(textTokenKey);

                return this;
            }
            
            public IRichTextEditorRow Build()
            {
                return new RichTextEditorRow(Key,
                    SequenceKey.NewSequenceKey(),
                    Map.ToImmutableDictionary(),
                    List.ToImmutableArray());
            }
        }
    }
}
