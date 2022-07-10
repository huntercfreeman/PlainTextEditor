using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RichTextEditor.ClassLib.Keyboard;
using RichTextEditor.ClassLib.Sequence;

namespace RichTextEditor.ClassLib.Store.RichTextEditorCase;

public partial record RichTextEditorStates
{
    private partial class StateMachine
    {
        public static RichTextEditorRecord HandleKeyDownEvent(RichTextEditorRecord focusedRichTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord)
        {
            if (KeyboardKeyFacts.IsWhitespaceKey(keyDownEventRecord))
            {
                return HandleWhitespace(focusedRichTextEditorRecord, keyDownEventRecord);
            }
            else if (KeyboardKeyFacts.IsMovementKey(keyDownEventRecord))
            {
                return HandleMovement(focusedRichTextEditorRecord, keyDownEventRecord);
            }
            else if (KeyboardKeyFacts.IsMetaKey(keyDownEventRecord)) 
            {
                return HandleMetaKey(focusedRichTextEditorRecord, keyDownEventRecord);
            }
            else
            {
                return HandleDefaultInsert(focusedRichTextEditorRecord, keyDownEventRecord);
            }
        }
        
        public static RichTextEditorRecord HandleOnClickEvent(RichTextEditorRecord focusedRichTextEditorRecord, 
            RichTextEditorOnClickAction richTextEditorOnClickAction)
        {
            var currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
    
            focusedRichTextEditorRecord = focusedRichTextEditorRecord with
            {
                CurrentTokenIndex = richTextEditorOnClickAction.TokenIndex,
                CurrentRowIndex = richTextEditorOnClickAction.RowIndex
            };

            currentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = richTextEditorOnClickAction.CharacterIndex ??
                        currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
        }

        private static RichTextEditorRecord InsertNewCurrentTokenAfterCurrentPosition(RichTextEditorRecord focusedRichTextEditorRecord,
            ITextToken textToken)
        {
            var replacementCurrentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);

            var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(
                focusedRichTextEditorRecord.CurrentRichTextEditorRow.Map
            );

            nextTokenMap[textToken.Key] = textToken;
            
            var nextTokenList = new List<TextTokenKey>(
                focusedRichTextEditorRecord.CurrentRichTextEditorRow.Array
            );

            nextTokenList.Insert(focusedRichTextEditorRecord.CurrentTokenIndex + 1, textToken.Key);

            var nextRowInstance = focusedRichTextEditorRecord.GetCurrentRichTextEditorRowAs<RichTextEditorRow>() with
            {
                Map = nextTokenMap.ToImmutableDictionary(),
                Array = nextTokenList.ToImmutableArray(),
                SequenceKey = SequenceKey.NewSequenceKey()
            };
            
            var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                focusedRichTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedRichTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary(),
                CurrentTokenIndex = focusedRichTextEditorRecord.CurrentTokenIndex + 1
            };
        }
        
        private static RichTextEditorRecord RemoveCurrentToken(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            if (focusedRichTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.StartOfRow)
                return RemoveStartOfRowToken(focusedRichTextEditorRecord);

            var toBeRemovedTokenKey = focusedRichTextEditorRecord.CurrentTextTokenKey;
            var toBeChangedRowKey = focusedRichTextEditorRecord.CurrentRichTextEditorRowKey;

            focusedRichTextEditorRecord = SetPreviousTokenAsCurrent(focusedRichTextEditorRecord);

            var nextRowInstance = focusedRichTextEditorRecord.Map[toBeChangedRowKey]
                .With()
                .Remove(toBeRemovedTokenKey)
                .Build();
            
            var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                focusedRichTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedRichTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary(),
            };
        }
        
        private static RichTextEditorRecord RemoveStartOfRowToken(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            if (focusedRichTextEditorRecord.CurrentRowIndex == 0)
            {
                return focusedRichTextEditorRecord;
            }
            
            if (focusedRichTextEditorRecord.CurrentRichTextEditorRow.Array.Length == 1)
            {
                return RemoveCurrentRow(focusedRichTextEditorRecord);
            }

            // TODO: Move current row to end of previous row
            return focusedRichTextEditorRecord;
        }

        private static RichTextEditorRecord RemoveCurrentRow(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var toBeDeletedRow = focusedRichTextEditorRecord.CurrentRichTextEditorRow;

            focusedRichTextEditorRecord = SetPreviousTokenAsCurrent(focusedRichTextEditorRecord);
            
            return (RichTextEditorRecord) focusedRichTextEditorRecord
                .With()
                .Remove(toBeDeletedRow.Key)
                .Build();
        }
        
        // The replacement token must have the same Key as the one being replaced
        private static RichTextEditorRecord ReplaceCurrentTokenWith(RichTextEditorRecord focusedRichTextEditorRecord,
            ITextToken textToken)
        {
            var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(
                focusedRichTextEditorRecord.CurrentRichTextEditorRow.Map
            );

            nextTokenMap[textToken.Key] = textToken;

            var nextRowInstance = focusedRichTextEditorRecord.GetCurrentRichTextEditorRowAs<RichTextEditorRow>() with
            {
                Map = nextTokenMap.ToImmutableDictionary(),
                SequenceKey = SequenceKey.NewSequenceKey()
            };
            
            var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                focusedRichTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedRichTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary()
            };
        }

        private static RichTextEditorRecord InsertNewLine(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var replacementCurrentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);

            var currentRow = focusedRichTextEditorRecord
                .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

            var replacementRowBuilder = currentRow.With();

            var constructedRowBuilder = new RichTextEditorRow().With();
            
            for (int i = focusedRichTextEditorRecord.CurrentTokenIndex + 1; i < currentRow.Array.Length; i++)
            {
                var tokenKey = currentRow.Array[i];
                var token = currentRow.Map[tokenKey];
                
                replacementRowBuilder.Remove(token.Key);

                constructedRowBuilder.Add(token);
            }

            var replacementRowInstance = replacementRowBuilder.Build();
            
            var constructedRowInstance = constructedRowBuilder.Build();

            return (RichTextEditorRecord) focusedRichTextEditorRecord
                .With()
                .Remove(replacementRowInstance.Key)
                .Insert(focusedRichTextEditorRecord.CurrentRowIndex, replacementRowInstance)
                .Insert(focusedRichTextEditorRecord.CurrentRowIndex + 1, constructedRowInstance)
                .CurrentTokenIndexOf(0)
                .CurrentRowIndexOf(focusedRichTextEditorRecord.CurrentRowIndex + 1)
                .Build();
        }
        
        private static (int rowIndex, int tokenIndex, TextTokenBase token) GetPreviousTokenTuple(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            if (focusedRichTextEditorRecord.CurrentTokenIndex == 0)
            {
                if (focusedRichTextEditorRecord.CurrentRowIndex > 0) 
                {
                    var rowIndex = focusedRichTextEditorRecord.CurrentRowIndex - 1;

                    var rowKey = focusedRichTextEditorRecord.Array[rowIndex];

                    var row = focusedRichTextEditorRecord.Map[rowKey];

                    var tokenIndex = row.Array.Length - 1;

                    var tokenKey = row.Array[tokenIndex];
                    
                    var token = row.Map[tokenKey];

                    return (
                        rowIndex, 
                        tokenIndex, 
                        token 
                            as TextTokenBase
                            ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                    );
                }

                return (
                    focusedRichTextEditorRecord.CurrentRowIndex, 
                    focusedRichTextEditorRecord.CurrentTokenIndex, 
                    focusedRichTextEditorRecord.GetCurrentTextTokenAs<TextTokenBase>()
                );
            }
            else
            {
                var row = focusedRichTextEditorRecord.GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

                var tokenIndex = focusedRichTextEditorRecord.CurrentTokenIndex - 1;

                var tokenKey = row.Array[tokenIndex];
                
                var token = row.Map[tokenKey];

                return (
                    focusedRichTextEditorRecord.CurrentRowIndex, 
                    tokenIndex, 
                    token 
                        as TextTokenBase
                        ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                );
            }
        }
        
        private static (int rowIndex, int tokenIndex, TextTokenBase token) GetNextTokenTuple(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var currentRow = focusedRichTextEditorRecord.GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

            if (focusedRichTextEditorRecord.CurrentTokenIndex == currentRow.Array.Length - 1)
            {
                if (focusedRichTextEditorRecord.CurrentRowIndex < focusedRichTextEditorRecord.Array.Length - 1) 
                {
                    var rowIndex = focusedRichTextEditorRecord.CurrentRowIndex + 1;

                    var rowKey = focusedRichTextEditorRecord.Array[rowIndex];

                    var row = focusedRichTextEditorRecord.Map[rowKey];

                    var tokenIndex = 0;

                    var tokenKey = row.Array[tokenIndex];
                    
                    var token = row.Map[tokenKey];

                    return (
                        rowIndex, 
                        tokenIndex, 
                        token 
                            as TextTokenBase
                            ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                    );
                }

                return (
                    focusedRichTextEditorRecord.CurrentRowIndex, 
                    focusedRichTextEditorRecord.CurrentTokenIndex, 
                    focusedRichTextEditorRecord.GetCurrentTextTokenAs<TextTokenBase>()
                );
            }
            else
            {
                var tokenIndex = focusedRichTextEditorRecord.CurrentTokenIndex + 1;

                var tokenKey = currentRow.Array[tokenIndex];
                
                var token = currentRow.Map[tokenKey];

                return (
                    focusedRichTextEditorRecord.CurrentRowIndex, 
                    tokenIndex, 
                    token 
                        as TextTokenBase
                        ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                );
            }
        }
        
        private static RichTextEditorRecord SetPreviousTokenAsCurrent(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var replacementCurrentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);

            var previousTokenTuple = GetPreviousTokenTuple(focusedRichTextEditorRecord);

            if (previousTokenTuple.rowIndex == focusedRichTextEditorRecord.CurrentRowIndex)
            {
                if (previousTokenTuple.token.Key == focusedRichTextEditorRecord.CurrentTextTokenKey)
                {
                    // No tokens previous to me
                    replacementCurrentToken = focusedRichTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>() with
                        {
                            IndexInPlainText = 0
                        };

                    return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
                }

                // There is a token previous to me on my current row
                var currentRow = focusedRichTextEditorRecord
                    .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(currentRow.Map);

                nextTokenMap[previousTokenTuple.token.Key] = previousTokenTuple.token with
                {
                    IndexInPlainText = previousTokenTuple.token.PlainText.Length - 1
                };

                var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                    focusedRichTextEditorRecord.Map
                );

                nextRowMap[focusedRichTextEditorRecord.CurrentRichTextEditorRowKey] = focusedRichTextEditorRecord
                    .GetCurrentRichTextEditorRowAs<RichTextEditorRow>() with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedRichTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = previousTokenTuple.tokenIndex
                };
            }
            else
            {
                // There was a previous token HOWEVER, it was located on previous row
                var previousRowKey = focusedRichTextEditorRecord.Array[previousTokenTuple.rowIndex];

                var previousRow = focusedRichTextEditorRecord.Map[previousRowKey]
                    as RichTextEditorRow
                    ?? throw new ApplicationException($"Expected {nameof(RichTextEditorRow)}");

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(previousRow.Map);

                nextTokenMap[previousTokenTuple.token.Key] = previousTokenTuple.token with
                {
                    IndexInPlainText = previousTokenTuple.token.PlainText.Length - 1
                };

                var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                    focusedRichTextEditorRecord.Map
                );

                nextRowMap[previousRowKey] = previousRow with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedRichTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = previousTokenTuple.tokenIndex,
                    CurrentRowIndex = previousTokenTuple.rowIndex
                };
            }
        }
        
        private static RichTextEditorRecord SetNextTokenAsCurrent(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var replacementCurrentToken = focusedRichTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedRichTextEditorRecord = ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);

            var nextTokenTuple = GetNextTokenTuple(focusedRichTextEditorRecord);

            if (nextTokenTuple.rowIndex == focusedRichTextEditorRecord.CurrentRowIndex)
            {
                if (nextTokenTuple.token.Key == focusedRichTextEditorRecord.CurrentTextTokenKey)
                {
                    // No tokens next to me
                    replacementCurrentToken = focusedRichTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>() with
                        {
                            IndexInPlainText = focusedRichTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                        };

                    return ReplaceCurrentTokenWith(focusedRichTextEditorRecord, replacementCurrentToken);
                }

                // There is a token next to me on my current row
                var currentRow = focusedRichTextEditorRecord
                    .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(currentRow.Map);

                nextTokenMap[nextTokenTuple.token.Key] = nextTokenTuple.token with
                {
                    IndexInPlainText = 0
                };

                var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                    focusedRichTextEditorRecord.Map
                );

                nextRowMap[focusedRichTextEditorRecord.CurrentRichTextEditorRowKey] = focusedRichTextEditorRecord
                    .GetCurrentRichTextEditorRowAs<RichTextEditorRow>() with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedRichTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = nextTokenTuple.tokenIndex
                };
            }
            else
            {
                // There was a next token HOWEVER, it was located on the next row
                var nextRowKey = focusedRichTextEditorRecord.Array[nextTokenTuple.rowIndex];

                var nextRow = focusedRichTextEditorRecord.Map[nextRowKey]
                    as RichTextEditorRow
                    ?? throw new ApplicationException($"Expected {nameof(RichTextEditorRow)}");

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(nextRow.Map);

                nextTokenMap[nextTokenTuple.token.Key] = nextTokenTuple.token with
                {
                    IndexInPlainText = 0
                };

                var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                    focusedRichTextEditorRecord.Map
                );

                nextRowMap[nextRowKey] = nextRow with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedRichTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = nextTokenTuple.tokenIndex,
                    CurrentRowIndex = nextTokenTuple.rowIndex
                };
            }
        }

        private static RichTextEditorRecord MoveCurrentRowToEndOfPreviousRow(RichTextEditorRecord focusedRichTextEditorRecord)
        {
            var toBeMovedRow = focusedRichTextEditorRecord
                .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

            focusedRichTextEditorRecord = SetPreviousTokenAsCurrent(focusedRichTextEditorRecord);

            var currentRow = focusedRichTextEditorRecord
                .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

            var replacementRowBuilder = currentRow.With();

            for (int i = 1; i < toBeMovedRow.Array.Length; i++)
            {
                var tokenKey = toBeMovedRow.Array[i];
                var token = toBeMovedRow.Map[tokenKey];
                
                replacementRowBuilder.Add(token);
            }

            var replacementRowInstance = replacementRowBuilder.Build();

            var nextRowMap = new Dictionary<RichTextEditorRowKey, IRichTextEditorRow>(
                focusedRichTextEditorRecord.Map
            );
            
            var nextRowList = new List<RichTextEditorRowKey>(
                focusedRichTextEditorRecord.Array
            );

            nextRowList.Remove(toBeMovedRow.Key);
            nextRowMap.Remove(toBeMovedRow.Key);
            
            nextRowMap[replacementRowInstance.Key] = replacementRowInstance;
            
            return focusedRichTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary(),
                Array = nextRowList.ToImmutableArray()
            };
        }

        /// <summary>
		/// Returns the inclusive starting column index
		/// </summary>
		/// <param name="nextPlainTextEditorState"></param>
		/// <returns></returns>
		private static int CalculateCurrentTokenColumnIndexRespectiveToRow(
			RichTextEditorRecord focusedRichTextEditorRecord)
		{
			var rollingCount = 0;
            var currentRow = focusedRichTextEditorRecord
                .GetCurrentRichTextEditorRowAs<RichTextEditorRow>();

			foreach (var tokenKey in currentRow.Array)
			{
				if (tokenKey == focusedRichTextEditorRecord.CurrentTextToken.Key)
                {
					return rollingCount;
				}
				else
				{
                    var token = currentRow.Map[tokenKey];
					rollingCount += token.PlainText.Length;
				}
			}

			return 0;
		}

        private static (int inclusiveStartingColumnIndex, int exclusiveEndingColumnIndex, TextTokenBase token) CalculateTokenAtColumnIndexRespectiveToRow(
			RichTextEditorRecord focusedRichTextEditorRecord,
			RichTextEditorRow row,
			int columnIndex)
		{
			var rollingCount = 0;

            for (int i = 0; i < row.Array.Length; i++)
			{
                TextTokenKey tokenKey = row.Array[i];
                ITextToken token = row.Map[tokenKey];

				rollingCount += token.PlainText.Length;

				if (rollingCount > columnIndex || (i == row.Array.Length - 1))
				{
                    return (
                        rollingCount - token.PlainText.Length,
                        rollingCount,
                        token as TextTokenBase
                            ?? throw new ApplicationException($"Expected type {nameof(TextTokenBase)}")
                    );
                }
			}

            throw new ApplicationException("Row was empty");
		}
    }
}
