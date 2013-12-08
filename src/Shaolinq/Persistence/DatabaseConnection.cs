﻿// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Shaolinq.Persistence.Sql;
using Shaolinq.Persistence.Sql.Linq;
using Platform;

namespace Shaolinq.Persistence
{
	public abstract class DatabaseConnection
		: IDisposable
	{
		public string PersistenceStoreName { get; set; }
		public SqlDialect SqlDialect { get; protected set; }
		public string SchemaNamePrefix { get; protected set; }
		public SqlDataTypeProvider SqlDataTypeProvider { get; protected set; }

		public virtual bool SupportsNestedReaders
		{
			get
			{
				return false;
			}
		}

		public virtual Pair<string, PropertyDescriptor>[] GetPersistedNames(DataAccessModel dataAccessModel, PropertyDescriptor propertyDescriptor)
		{
			if (propertyDescriptor.IsBackReferenceProperty)
			{
				var i = 0;
				var typeDescriptor = dataAccessModel.GetTypeDescriptor(propertyDescriptor.PropertyType);

				var retval = new Pair<string, PropertyDescriptor>[typeDescriptor.PrimaryKeyProperties.Count()];

				foreach (var relatedPropertyDescriptor in typeDescriptor.PrimaryKeyProperties)
				{
					retval[i] = new Pair<string, PropertyDescriptor>(propertyDescriptor.PersistedName + relatedPropertyDescriptor.PersistedShortName, relatedPropertyDescriptor);

					i++;
				}

				return retval;
			}
			else if (propertyDescriptor.PersistedMemberAttribute != null && propertyDescriptor.PropertyType.IsDataAccessObjectType())
			{
				var i = 0;
				var typeDescriptor = dataAccessModel.GetTypeDescriptor(propertyDescriptor.PropertyType);

				var retval = new Pair<string, PropertyDescriptor>[typeDescriptor.PrimaryKeyProperties.Count];

				foreach (var relatedPropertyDescriptor in typeDescriptor.PrimaryKeyProperties)
				{
					retval[i] = new Pair<string, PropertyDescriptor>(propertyDescriptor.PersistedName + relatedPropertyDescriptor.PersistedShortName, relatedPropertyDescriptor);

					i++;
				}

				return retval;
			}
			else
			{
				return new[] { new Pair<string, PropertyDescriptor>(propertyDescriptor.PersistedName, propertyDescriptor) };
			}
		}

		public abstract Sql92QueryFormatter NewQueryFormatter(DataAccessModel dataAccessModel, SqlDataTypeProvider sqlDataTypeProvider, SqlDialect sqlDialect, Expression expression, SqlQueryFormatterOptions options);
		public abstract IPersistenceQueryProvider NewQueryProvider(DataAccessModel dataAccessModel, DatabaseConnection databaseConnection);
		public abstract DatabaseTransactionContext NewDataTransactionContext(DataAccessModel dataAccessModel, Transaction transaction);
		public abstract DatabaseCreator NewDatabaseCreator(DataAccessModel model);
		public abstract MigrationPlanApplicator NewMigrationPlanApplicator(DataAccessModel model);
		public abstract MigrationPlanCreator NewMigrationPlanCreator(DataAccessModel model);

		public abstract void DropAllConnections();
		
		public virtual void Dispose()
		{
			DropAllConnections();
		}
	}
}