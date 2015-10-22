﻿using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Shaolinq.Persistence.Computed
{
	public class ComputedExpressionTokenizer
	{
		private int currentChar;
		private readonly TextReader reader;
		private readonly StringBuilder stringBuilder;
		public string CurrentString { get; private set; }
		public string CurrentIdentifier { get; private set; }
		public long CurrentInteger { get; private set; }
		public string CurrentKey { get { return this.CurrentIdentifier; } }
		public ComputedExpressionToken CurrentToken { get; private set; }

		public bool CurrentTokenMatches(params ComputedExpressionToken[] tokens)
		{
			return tokens.Contains(this.CurrentToken);
		}

		public ComputedExpressionTokenizer(TextReader reader)
		{
			this.reader = reader;
			this.stringBuilder = new StringBuilder();

			this.ConsumeChar();
		}

		private void ConsumeChar()
		{
			this.currentChar = this.reader.Read();
		}

		public ComputedExpressionToken ReadNextToken()
		{
			while (Char.IsWhiteSpace((char)this.currentChar) || this.currentChar == '\0')
			{
				this.ConsumeChar();
			}

			if (this.currentChar == -1)
			{
				this.CurrentToken = ComputedExpressionToken.Eof;

				return this.CurrentToken;
			}

			if (this.currentChar == '=')
			{
				this.ConsumeChar();

				if (this.currentChar == '=')
				{
					this.ConsumeChar();

					this.CurrentToken = ComputedExpressionToken.Equals;
				}
				else
				{
					this.CurrentToken = ComputedExpressionToken.Assign;
				}

				return this.CurrentToken;
			}
			else if (this.currentChar == '?')
			{
				this.ConsumeChar();

				if (this.currentChar == '?')
				{
					this.ConsumeChar();
					this.CurrentToken = ComputedExpressionToken.DoubleQuestionMark;
				}
				else
				{
					this.CurrentToken = ComputedExpressionToken.QuestionMark;
				}

				return this.CurrentToken;
			}
			else if (this.currentChar == '!')
			{
				this.ConsumeChar();

				if (this.currentChar == '=')
				{
					this.ConsumeChar();

					this.CurrentToken = ComputedExpressionToken.NotEquals;
				}
				else
				{
					this.CurrentToken = ComputedExpressionToken.LogicalNot;
				}

				return this.CurrentToken;
			}
			else if (this.currentChar == '<')
			{
				this.ConsumeChar();

				if (this.currentChar == '=')
				{
					this.ConsumeChar();

					this.CurrentToken = ComputedExpressionToken.LessThanOrEqual;
				}
				else
				{
					this.CurrentToken = ComputedExpressionToken.LessThan;
				}

				return this.CurrentToken;
			}
			else if (this.currentChar == '>')
			{
				this.ConsumeChar();

				if (this.currentChar == '=')
				{
					this.ConsumeChar();

					this.CurrentToken = ComputedExpressionToken.GreaterThanOrEqual;
				}
				else
				{
					this.CurrentToken = ComputedExpressionToken.GreaterThan;
				}

				return this.CurrentToken;
			}
			else if (this.currentChar == '.')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.Period;

				return this.CurrentToken;
			}
			else if (this.currentChar == ';')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.Semicolon;

				return this.CurrentToken;
			}
			else if (this.currentChar == ',')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.Comma;

				return this.CurrentToken;
			}
			else if (this.currentChar == '(')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.LeftParen;

				return this.CurrentToken;
			}
			else if (this.currentChar == ')')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.RightParen;

				return this.CurrentToken;
			}
			else if (this.currentChar == '+')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.Add;

				return this.CurrentToken;
			}
			else if (this.currentChar == '-')
			{
				this.ConsumeChar();
				this.CurrentToken = ComputedExpressionToken.Subtract;

				return this.CurrentToken;
			}
			else if (Char.IsDigit((char)this.currentChar))
			{
				this.stringBuilder.Length = 0;

				while (this.currentChar != -1 && (Char.IsLetterOrDigit((char)this.currentChar)))
				{
					this.stringBuilder.Append((char)this.currentChar);

					this.ConsumeChar();
				}

				var s = this.stringBuilder.ToString();

				this.CurrentToken = ComputedExpressionToken.IntegerLiteral;
				this.CurrentInteger = Convert.ToInt64(s);

				return this.CurrentToken;
			}
			else if (Char.IsLetter((char)this.currentChar))
			{
				this.stringBuilder.Length = 0;

				while (this.currentChar != -1 && (Char.IsLetterOrDigit((char)this.currentChar) ))
				{
					this.stringBuilder.Append((char)this.currentChar);

					this.ConsumeChar();
				}

				var s = this.stringBuilder.ToString();

				this.CurrentToken = ComputedExpressionToken.Identifier;
				this.CurrentIdentifier = s;

				return this.CurrentToken;
			}
			else if (this.currentChar == '"')
			{
				var previousChar = -1;
				var startChar = this.currentChar;

				this.stringBuilder.Length = 0;

				this.ConsumeChar();

				while (this.currentChar != -1 && (this.currentChar != startChar || (this.currentChar == startChar && previousChar == '\\')))
				{
					if (this.currentChar == startChar)
					{
						if (previousChar == '\\')
						{
							this.stringBuilder.Remove(this.stringBuilder.Length - 1, 1);
							this.stringBuilder.Append(startChar);
							this.ConsumeChar();

							continue;
						}
						else
						{
							break;
						}
					}

					this.stringBuilder.Append((char)this.currentChar);
					previousChar = this.currentChar;

					this.ConsumeChar();
				}

				if (this.currentChar == startChar)
				{
					this.ConsumeChar();
				}

				this.CurrentToken = ComputedExpressionToken.StringLiteral;
				this.CurrentString = this.stringBuilder.ToString();

				return this.CurrentToken;
			}
			
			throw new InvalidOperationException(string.Format("Unexpected character: {0}", (char)this.currentChar));
		}
	}
}