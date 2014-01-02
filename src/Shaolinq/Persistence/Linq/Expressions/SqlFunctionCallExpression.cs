// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

﻿using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Shaolinq.Persistence.Linq.Expressions
{
	/// <summary>
	/// Represents an SQL function call such as DATE, YEAR, SUBSTRING or ISNULL.
	/// </summary>
	public class SqlFunctionCallExpression
		: SqlBaseExpression
	{
		public SqlFunction Function { get; private set; }
		public ReadOnlyCollection<Expression> Arguments { get; private set; }

		public override ExpressionType NodeType
		{
			get
			{
				return (ExpressionType)SqlExpressionType.FunctionCall;
			}
		}

		public SqlFunctionCallExpression(Type type, SqlFunction function, params Expression[] arguments)
			: base(type)
		{
			this.Function = function;
			this.Arguments = new ReadOnlyCollection<Expression>(arguments);
		}
	}
}