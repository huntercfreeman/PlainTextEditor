using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlainTextEditor.ClassLib.Keyboard;
using PlainTextEditor.ClassLib.Sequence;

namespace PlainTextEditor.ClassLib.Store.PlainTextEditorCase;

public partial record PlainTextEditorStates
{
    private partial class StateMachine
    {
        public static PlainTextEditorRecord HandleKeyDownEvent(PlainTextEditorRecord focusedPlainTextEditorRecord, 
            KeyDownEventRecord keyDownEventRecord)
        {
            if (KeyboardKeyFacts.IsWhitespaceKey(keyDownEventRecord))
            {
                return HandleWhitespace(focusedPlainTextEditorRecord, keyDownEventRecord);
            }
            else if (KeyboardKeyFacts.IsMovementKey(keyDownEventRecord))
            {
                return HandleMovement(focusedPlainTextEditorRecord, keyDownEventRecord);
            }
            else if (KeyboardKeyFacts.IsMetaKey(keyDownEventRecord)) 
            {
                return HandleMetaKey(focusedPlainTextEditorRecord, keyDownEventRecord);
            }
            else
            {
                return HandleDefaultInsert(focusedPlainTextEditorRecord, keyDownEventRecord);
            }
        }
        
        public static PlainTextEditorRecord HandleOnClickEvent(PlainTextEditorRecord focusedPlainTextEditorRecord, 
            PlainTextEditorOnClickAction plainTextEditorOnClickAction)
        {
            var currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            var replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = null
                };
            
            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
    
            focusedPlainTextEditorRecord = focusedPlainTextEditorRecord with
            {
                CurrentTokenIndex = plainTextEditorOnClickAction.TokenIndex,
                CurrentRowIndex = plainTextEditorOnClickAction.RowIndex
            };

            currentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>();

            replacementCurrentToken = currentToken with
                {
                    IndexInPlainText = plainTextEditorOnClickAction.CharacterIndex ??
                        currentToken.PlainText.Length - 1
                };

            return ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
        }

        private static PlainTextEditorRecord InsertNewCurrentTokenAfterCurrentPosition(PlainTextEditorRecord focusedPlainTextEditorRecord,
            ITextToken textToken)
        {
            var replacementCurrentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);

            var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(
                focusedPlainTextEditorRecord.CurrentPlainTextEditorRow.Map
            );

            nextTokenMap[textToken.Key] = textToken;
            
            var nextTokenList = new List<TextTokenKey>(
                focusedPlainTextEditorRecord.CurrentPlainTextEditorRow.Array
            );

            nextTokenList.Insert(focusedPlainTextEditorRecord.CurrentTokenIndex + 1, textToken.Key);

            var nextRowInstance = focusedPlainTextEditorRecord.GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>() with
            {
                Map = nextTokenMap.ToImmutableDictionary(),
                Array = nextTokenList.ToImmutableArray(),
                SequenceKey = SequenceKey.NewSequenceKey()
            };
            
            var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                focusedPlainTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedPlainTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary(),
                CurrentTokenIndex = focusedPlainTextEditorRecord.CurrentTokenIndex + 1
            };
        }
        
        private static PlainTextEditorRecord RemoveCurrentToken(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentTextToken.Kind == TextTokenKind.StartOfRow)
                return RemoveStartOfRowToken(focusedPlainTextEditorRecord);

            var toBeRemovedTokenKey = focusedPlainTextEditorRecord.CurrentTextTokenKey;
            var toBeChangedRowKey = focusedPlainTextEditorRecord.CurrentPlainTextEditorRowKey;

            focusedPlainTextEditorRecord = SetPreviousTokenAsCurrent(focusedPlainTextEditorRecord);

            var nextRowInstance = focusedPlainTextEditorRecord.Map[toBeChangedRowKey]
                .With()
                .Remove(toBeRemovedTokenKey)
                .Build();
            
            var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                focusedPlainTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedPlainTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary(),
            };
        }
        
        private static PlainTextEditorRecord RemoveStartOfRowToken(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentRowIndex == 0)
            {
                return focusedPlainTextEditorRecord;
            }
            
            if (focusedPlainTextEditorRecord.CurrentPlainTextEditorRow.Array.Length == 1)
            {
                return RemoveCurrentRow(focusedPlainTextEditorRecord);
            }

            // TODO: Move current row to end of previous row
            return focusedPlainTextEditorRecord;
        }

        private static PlainTextEditorRecord RemoveCurrentRow(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var toBeDeletedRow = focusedPlainTextEditorRecord.CurrentPlainTextEditorRow;

            focusedPlainTextEditorRecord = SetPreviousTokenAsCurrent(focusedPlainTextEditorRecord);
            
            return (PlainTextEditorRecord) focusedPlainTextEditorRecord
                .With()
                .Remove(toBeDeletedRow.Key)
                .Build();
        }
        
        // The replacement token must have the same Key as the one being replaced
        private static PlainTextEditorRecord ReplaceCurrentTokenWith(PlainTextEditorRecord focusedPlainTextEditorRecord,
            ITextToken textToken)
        {
            var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(
                focusedPlainTextEditorRecord.CurrentPlainTextEditorRow.Map
            );

            nextTokenMap[textToken.Key] = textToken;

            var nextRowInstance = focusedPlainTextEditorRecord.GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>() with
            {
                Map = nextTokenMap.ToImmutableDictionary(),
                SequenceKey = SequenceKey.NewSequenceKey()
            };
            
            var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                focusedPlainTextEditorRecord.Map
            );

            nextRowMap[nextRowInstance.Key] = nextRowInstance;

            return focusedPlainTextEditorRecord with
            {
                Map = nextRowMap.ToImmutableDictionary()
            };
        }

        private static PlainTextEditorRecord InsertNewLine(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var replacementCurrentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);

            var currentRow = focusedPlainTextEditorRecord
                .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

            var replacementRowBuilder = currentRow.With();

            var constructedRowBuilder = new PlainTextEditorRow().With();
            
            for (int i = focusedPlainTextEditorRecord.CurrentTokenIndex + 1; i < currentRow.Array.Length; i++)
            {
                var tokenKey = currentRow.Array[i];
                var token = currentRow.Map[tokenKey];
                
                replacementRowBuilder.Remove(token.Key);

                constructedRowBuilder.Add(token);
            }

            var replacementRowInstance = replacementRowBuilder.Build();
            
            var constructedRowInstance = constructedRowBuilder.Build();

            return (PlainTextEditorRecord) focusedPlainTextEditorRecord
                .With()
                .Remove(replacementRowInstance.Key)
                .Insert(focusedPlainTextEditorRecord.CurrentRowIndex, replacementRowInstance)
                .Insert(focusedPlainTextEditorRecord.CurrentRowIndex + 1, constructedRowInstance)
                .CurrentTokenIndexOf(0)
                .CurrentRowIndexOf(focusedPlainTextEditorRecord.CurrentRowIndex + 1)
                .Build();
        }
        
        private static (int rowIndex, int tokenIndex, TextTokenBase token) GetPreviousTokenTuple(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            if (focusedPlainTextEditorRecord.CurrentTokenIndex == 0)
            {
                if (focusedPlainTextEditorRecord.CurrentRowIndex > 0) 
                {
                    var rowIndex = focusedPlainTextEditorRecord.CurrentRowIndex - 1;

                    var rowKey = focusedPlainTextEditorRecord.Array[rowIndex];

                    var row = focusedPlainTextEditorRecord.Map[rowKey];

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
                    focusedPlainTextEditorRecord.CurrentRowIndex, 
                    focusedPlainTextEditorRecord.CurrentTokenIndex, 
                    focusedPlainTextEditorRecord.GetCurrentTextTokenAs<TextTokenBase>()
                );
            }
            else
            {
                var row = focusedPlainTextEditorRecord.GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

                var tokenIndex = focusedPlainTextEditorRecord.CurrentTokenIndex - 1;

                var tokenKey = row.Array[tokenIndex];
                
                var token = row.Map[tokenKey];

                return (
                    focusedPlainTextEditorRecord.CurrentRowIndex, 
                    tokenIndex, 
                    token 
                        as TextTokenBase
                        ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                );
            }
        }
        
        private static (int rowIndex, int tokenIndex, TextTokenBase token) GetNextTokenTuple(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var currentRow = focusedPlainTextEditorRecord.GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

            if (focusedPlainTextEditorRecord.CurrentTokenIndex == currentRow.Array.Length - 1)
            {
                if (focusedPlainTextEditorRecord.CurrentRowIndex < focusedPlainTextEditorRecord.Array.Length - 1) 
                {
                    var rowIndex = focusedPlainTextEditorRecord.CurrentRowIndex + 1;

                    var rowKey = focusedPlainTextEditorRecord.Array[rowIndex];

                    var row = focusedPlainTextEditorRecord.Map[rowKey];

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
                    focusedPlainTextEditorRecord.CurrentRowIndex, 
                    focusedPlainTextEditorRecord.CurrentTokenIndex, 
                    focusedPlainTextEditorRecord.GetCurrentTextTokenAs<TextTokenBase>()
                );
            }
            else
            {
                var tokenIndex = focusedPlainTextEditorRecord.CurrentTokenIndex + 1;

                var tokenKey = currentRow.Array[tokenIndex];
                
                var token = currentRow.Map[tokenKey];

                return (
                    focusedPlainTextEditorRecord.CurrentRowIndex, 
                    tokenIndex, 
                    token 
                        as TextTokenBase
                        ?? throw new ApplicationException($"Expected {nameof(TextTokenBase)}")
                );
            }
        }
        
        private static PlainTextEditorRecord SetPreviousTokenAsCurrent(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var replacementCurrentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);

            var previousTokenTuple = GetPreviousTokenTuple(focusedPlainTextEditorRecord);

            if (previousTokenTuple.rowIndex == focusedPlainTextEditorRecord.CurrentRowIndex)
            {
                if (previousTokenTuple.token.Key == focusedPlainTextEditorRecord.CurrentTextTokenKey)
                {
                    // No tokens previous to me
                    replacementCurrentToken = focusedPlainTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>() with
                        {
                            IndexInPlainText = 0
                        };

                    return ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
                }

                // There is a token previous to me on my current row
                var currentRow = focusedPlainTextEditorRecord
                    .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(currentRow.Map);

                nextTokenMap[previousTokenTuple.token.Key] = previousTokenTuple.token with
                {
                    IndexInPlainText = previousTokenTuple.token.PlainText.Length - 1
                };

                var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                    focusedPlainTextEditorRecord.Map
                );

                nextRowMap[focusedPlainTextEditorRecord.CurrentPlainTextEditorRowKey] = focusedPlainTextEditorRecord
                    .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>() with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedPlainTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = previousTokenTuple.tokenIndex
                };
            }
            else
            {
                // There was a previous token HOWEVER, it was located on previous row
                var previousRowKey = focusedPlainTextEditorRecord.Array[previousTokenTuple.rowIndex];

                var previousRow = focusedPlainTextEditorRecord.Map[previousRowKey]
                    as PlainTextEditorRow
                    ?? throw new ApplicationException($"Expected {nameof(PlainTextEditorRow)}");

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(previousRow.Map);

                nextTokenMap[previousTokenTuple.token.Key] = previousTokenTuple.token with
                {
                    IndexInPlainText = previousTokenTuple.token.PlainText.Length - 1
                };

                var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                    focusedPlainTextEditorRecord.Map
                );

                nextRowMap[previousRowKey] = previousRow with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedPlainTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = previousTokenTuple.tokenIndex,
                    CurrentRowIndex = previousTokenTuple.rowIndex
                };
            }
        }
        
        private static PlainTextEditorRecord SetNextTokenAsCurrent(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var replacementCurrentToken = focusedPlainTextEditorRecord
                .GetCurrentTextTokenAs<TextTokenBase>() with
                {
                    IndexInPlainText = null
                };

            focusedPlainTextEditorRecord = ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);

            var nextTokenTuple = GetNextTokenTuple(focusedPlainTextEditorRecord);

            if (nextTokenTuple.rowIndex == focusedPlainTextEditorRecord.CurrentRowIndex)
            {
                if (nextTokenTuple.token.Key == focusedPlainTextEditorRecord.CurrentTextTokenKey)
                {
                    // No tokens next to me
                    replacementCurrentToken = focusedPlainTextEditorRecord
                        .GetCurrentTextTokenAs<TextTokenBase>() with
                        {
                            IndexInPlainText = focusedPlainTextEditorRecord.CurrentTextToken.PlainText.Length - 1
                        };

                    return ReplaceCurrentTokenWith(focusedPlainTextEditorRecord, replacementCurrentToken);
                }

                // There is a token next to me on my current row
                var currentRow = focusedPlainTextEditorRecord
                    .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(currentRow.Map);

                nextTokenMap[nextTokenTuple.token.Key] = nextTokenTuple.token with
                {
                    IndexInPlainText = 0
                };

                var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                    focusedPlainTextEditorRecord.Map
                );

                nextRowMap[focusedPlainTextEditorRecord.CurrentPlainTextEditorRowKey] = focusedPlainTextEditorRecord
                    .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>() with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedPlainTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = nextTokenTuple.tokenIndex
                };
            }
            else
            {
                // There was a next token HOWEVER, it was located on the next row
                var nextRowKey = focusedPlainTextEditorRecord.Array[nextTokenTuple.rowIndex];

                var nextRow = focusedPlainTextEditorRecord.Map[nextRowKey]
                    as PlainTextEditorRow
                    ?? throw new ApplicationException($"Expected {nameof(PlainTextEditorRow)}");

                var nextTokenMap = new Dictionary<TextTokenKey, ITextToken>(nextRow.Map);

                nextTokenMap[nextTokenTuple.token.Key] = nextTokenTuple.token with
                {
                    IndexInPlainText = 0
                };

                var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                    focusedPlainTextEditorRecord.Map
                );

                nextRowMap[nextRowKey] = nextRow with
                    {
                        Map = nextTokenMap.ToImmutableDictionary(),
                        SequenceKey = SequenceKey.NewSequenceKey()
                    };

                return focusedPlainTextEditorRecord with
                {
                    Map = nextRowMap.ToImmutableDictionary(),
                    CurrentTokenIndex = nextTokenTuple.tokenIndex,
                    CurrentRowIndex = nextTokenTuple.rowIndex
                };
            }
        }

        private static PlainTextEditorRecord MoveCurrentRowToEndOfPreviousRow(PlainTextEditorRecord focusedPlainTextEditorRecord)
        {
            var toBeMovedRow = focusedPlainTextEditorRecord
                .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

            focusedPlainTextEditorRecord = SetPreviousTokenAsCurrent(focusedPlainTextEditorRecord);

            var currentRow = focusedPlainTextEditorRecord
                .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

            var replacementRowBuilder = currentRow.With();

            for (int i = 1; i < toBeMovedRow.Array.Length; i++)
            {
                var tokenKey = toBeMovedRow.Array[i];
                var token = toBeMovedRow.Map[tokenKey];
                
                replacementRowBuilder.Add(token);
            }

            var replacementRowInstance = replacementRowBuilder.Build();

            var nextRowMap = new Dictionary<PlainTextEditorRowKey, IPlainTextEditorRow>(
                focusedPlainTextEditorRecord.Map
            );
            
            var nextRowList = new List<PlainTextEditorRowKey>(
                focusedPlainTextEditorRecord.Array
            );

            nextRowList.Remove(toBeMovedRow.Key);
            nextRowMap.Remove(toBeMovedRow.Key);
            
            nextRowMap[replacementRowInstance.Key] = replacementRowInstance;
            
            return focusedPlainTextEditorRecord with
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
			PlainTextEditorRecord focusedPlainTextEditorRecord)
		{
			var rollingCount = 0;
            var currentRow = focusedPlainTextEditorRecord
                .GetCurrentPlainTextEditorRowAs<PlainTextEditorRow>();

			foreach (var tokenKey in currentRow.Array)
			{
				if (tokenKey == focusedPlainTextEditorRecord.CurrentTextToken.Key)
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
			PlainTextEditorRecord focusedPlainTextEditorRecord,
			PlainTextEditorRow row,
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