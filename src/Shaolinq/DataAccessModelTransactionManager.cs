﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;

namespace Shaolinq
{
	public class DataAccessModelTransactionManager
		: IDisposable
	{
		/// <summary>
		/// Gets the <see cref="DataAccessModelTransactionManager "/> for the current <see cref="BaseDataAccessModel"/> for the current thread.
		/// </summary>
		/// <remarks>
		/// The framework does not support accessing objects created within different transactions
		/// from other transactions. For each Transaction there may be more than one attached
		/// <see cref="TransactionContext"/> (one for each thread that participates in the transaction).
		/// </remarks>
		public static DataAccessModelTransactionManager GetAmbientTransactionManager(BaseDataAccessModel dataAccessModel)
		{
			DataAccessModelTransactionManager retval;
			var transactionManagers = t_AmbientTransactionManagers;

			if (transactionManagers == null)
			{
				transactionManagers = new Dictionary<BaseDataAccessModel, DataAccessModelTransactionManager>();
				t_AmbientTransactionManagers = transactionManagers;
			}

			if (!transactionManagers.TryGetValue(dataAccessModel, out retval))
			{
				retval = new DataAccessModelTransactionManager(dataAccessModel);

				transactionManagers[dataAccessModel] = retval;
			}

			return retval;
		}
		[ThreadStatic]
		private volatile static Dictionary<BaseDataAccessModel, DataAccessModelTransactionManager> t_AmbientTransactionManagers;
        
		public BaseDataAccessModel DataAccessModel
		{
			get;
			private set;
		}

		~DataAccessModelTransactionManager()
		{
			this.Dispose();
		}
        
		public void FlushConnections()
		{
			if (this.rootContext != null)
			{
				this.rootContext.FlushConnections();
			}
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref this.disposed, 1, 0) != 0)
			{
				return;
			}

			if (this.rootContext != null)
			{
				this.rootContext.Dispose();
			}

			GC.SuppressFinalize(this);
		}

		private int disposed = 0;
		private TransactionContext rootContext;
		private readonly IDictionary<Transaction, TransactionContext> transactionContextsByTransaction;

		public DataAccessModelTransactionManager(BaseDataAccessModel dataAccessModel)
		{
			EventHandler handler = null;
			var weakThis = new WeakReference(this);

			this.DataAccessModel = dataAccessModel;
			transactionContextsByTransaction = new Dictionary<Transaction, TransactionContext>();
            
			handler = delegate(object sender, EventArgs eventArgs)
			{
				var strongThis = (DataAccessModelTransactionManager)weakThis.Target;

				if (strongThis != null)
				{
					strongThis.Dispose();
				}
				else
				{
					((BaseDataAccessModel)sender).Disposed -= handler;
				}
			};

			this.DataAccessModel.Disposed += handler;
		}

		public virtual TransactionContext GetCurrentContext(bool forWrite)
		{
			TransactionContext retval;

			if (!TryGetCurrentContext(forWrite, out retval))
			{
				throw new NotSupportedException("Write operation must be performed inside a transaction context");
			}
			else
			{
				return retval;
			}
		}

		public virtual bool TryGetCurrentContext(bool forWrite, out TransactionContext retval)
		{
			Transaction transaction;

			try
			{
				transaction = Transaction.Current;
			}
			catch (InvalidOperationException)
			{
				transaction = Transaction.Current = null;
			}

			if (transaction == null && forWrite)
			{
				retval = null;

				return false;
			}

			if (transaction != null)
			{
				if (transaction.TransactionInformation.Status == TransactionStatus.Aborted)
				{
					throw new TransactionAbortedException();
				}

				if (!transactionContextsByTransaction.TryGetValue(transaction, out retval))
				{
					retval = new TransactionContext(this.DataAccessModel, transaction);
                    
					transaction.TransactionCompleted += delegate
					{
						transactionContextsByTransaction.Remove(transaction);
					};

					transaction.EnlistVolatile(retval, EnlistmentOptions.None);
					transactionContextsByTransaction[transaction] = retval;
				}
			}
			else
			{
				if (rootContext == null)
				{
					retval = new TransactionContext(this.DataAccessModel, null);

					rootContext = retval;
				}
				else
				{
					retval = rootContext;
				}
			}

			return true;
		}
	}
}