﻿using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq
{
	public static class ExpressionExtensions
	{
		public static LambdaExpression StripQuotes(this Expression expression)
		{
			while (expression.NodeType == ExpressionType.Quote)
			{
				expression = ((UnaryExpression)expression).Operand;
			}

			return (LambdaExpression)expression;
		}
	}
}