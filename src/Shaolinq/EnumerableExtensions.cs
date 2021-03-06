﻿// Copyright (c) 2007-2016 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Shaolinq.Persistence;

namespace Shaolinq
{
	public static partial class EnumerableExtensions
	{
		[RewriteAsync]
		internal static T AlwaysReadFirst<T>(this IEnumerable<T> source)
		{
			return source.First();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IEnumerable<T?> DefaultIfEmptyCoalesceSpecifiedValue<T>(this IEnumerable<T?> source, T? specifiedValue)
			where T : struct => source.DefaultIfEmptyCoalesceSpecifiedValueAsync(specifiedValue);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IAsyncEnumerable<T?> DefaultIfEmptyCoalesceSpecifiedValueAsync<T>(this IEnumerable<T?> source, T? specifiedValue)
			where T : struct => new AsyncEnumerableAdapter<T?>(() => new DefaultIfEmptyCoalesceSpecifiedValueEnumerator<T>(source.GetAsyncEnumerator(), specifiedValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IAsyncEnumerable<T> DefaultIfEmptyAsync<T>(this IEnumerable<T> source)
			=> source.DefaultIfEmptyAsync(default(T));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IAsyncEnumerable<T> DefaultIfEmptyAsync<T>(this IEnumerable<T> source, T defaultValue)
			=> new AsyncEnumerableAdapter<T>(() => new DefaultIfEmptyEnumerator<T>(source.GetAsyncEnumerator(), defaultValue));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IEnumerable<T> EmptyIfFirstIsNull<T>(this IEnumerable<T> source) 
			=> new AsyncEnumerableAdapter<T>(() => new EmptyIfFirstIsNullEnumerator<T>(source.GetAsyncEnumerator()));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IAsyncEnumerable<T> EmptyIfFirstIsNullAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken) 
			=> new AsyncEnumerableAdapter<T>(() => new EmptyIfFirstIsNullEnumerator<T>(source.GetAsyncEnumerator()));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static IEnumerator<T> GetEnumeratorEx<T>(this IEnumerable<T> source) 
			=> source.GetEnumerator();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task<IAsyncEnumerator<T>> GetEnumeratorExAsync<T>(this IEnumerable<T> source) 
			=> Task.FromResult(source.GetAsyncEnumerator());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool MoveNextEx<T>(this IEnumerator<T> enumerator)
			=> enumerator.MoveNext();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static Task<bool> MoveNextExAsync<T>(this IAsyncEnumerator<T> enumerator, CancellationToken cancellationToken)
			=> enumerator.MoveNextAsync(cancellationToken);
		
		internal static IAsyncEnumerator<T> GetAsyncEnumerator<T>(this IEnumerable<T> source)
		{
			var internalAsyncEnumerable = source as IInternalAsyncEnumerable<T>;

			if (internalAsyncEnumerable != null)
			{
				return internalAsyncEnumerable.GetAsyncEnumerator();
			}

			var asyncEnumerable = source as IAsyncEnumerable<T>;

			if (asyncEnumerable != null)
			{
				return asyncEnumerable.GetAsyncEnumerator();
			}

		   return new AsyncEnumeratorAdapter<T>(source.GetEnumerator());
		}

		internal static IAsyncEnumerator<T> GetAsyncEnumeratorOrThrow<T>(this IEnumerable<T> source)
		{
			var asyncEnumerable = source as IAsyncEnumerable<T>;

			if (asyncEnumerable == null)
			{
				throw new NotSupportedException($"The given enumerable {source.GetType().Name} does not support {nameof(IAsyncEnumerable<T>)}");
			}

			return asyncEnumerable.GetAsyncEnumerator();
		}

		[RewriteAsync]
		private static int Count<T>(this IEnumerable<T> source)
		{
			var list = source as ICollection<T>;

			if (list != null)
			{
				return list.Count;
			}

			var retval = 0;

			using (var enumerator = source.GetEnumeratorEx())
			{
				while (enumerator.MoveNextEx())
				{
					retval++;
				}
			}

			return retval;
		}

		[RewriteAsync]
		private static long LongCount<T>(this IEnumerable<T> source)
		{
			var list = source as ICollection<T>;

			if (list != null)
			{
				return list.Count;
			}

			var retval = 0L;

			using (var enumerator = source.GetEnumeratorEx())
			{
				while (enumerator.MoveNextEx())
				{
					retval++;
				}
			}

			return retval;
		}

		[RewriteAsync]
		internal static T SingleOrSpecifiedValueIfFirstIsDefaultValue<T>(this IEnumerable<T> source, T specifiedValue)
		{
			using (var enumerator = source.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx())
				{
					return Enumerable.Single<T>(Enumerable.Empty<T>());
				}

				var result = enumerator.Current;

				if (enumerator.MoveNextEx())
				{
					return Enumerable.Single<T>(new T[2]);
				}

				if (object.Equals(result, default(T)))
				{
					return specifiedValue;
				}

				return result;
			}
		}

		[RewriteAsync]
		private static T Single<T>(this IEnumerable<T> source)
		{
			using (var enumerator = source.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx())
				{
					return Enumerable.Single<T>(Enumerable.Empty<T>());
				}

				var result = enumerator.Current;

				if (enumerator.MoveNextEx())
				{
					return Enumerable.Single<T>(new T[2]);
				}

				return result;
			}
		}

		[RewriteAsync()]
		private static T SingleOrDefault<T>(this IEnumerable<T> source)
		{
			using (var enumerator = source.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx())
				{
					return default(T);
				}

				var result = enumerator.Current;

				if (enumerator.MoveNextEx())
				{
					return Enumerable.Single(new T[2]);
				}

				return result;
			}
		}

		[RewriteAsync]
		private static T First<T>(this IEnumerable<T> enumerable)
		{
			using (var enumerator = enumerable.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx())
				{
					return Enumerable.First(Enumerable.Empty<T>());
				}

				return enumerator.Current;
			}
		}

		[RewriteAsync]
		private static T FirstOrDefault<T>(this IEnumerable<T> source)
		{
			using (var enumerator = source.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx())
				{
					return default(T);
				}

				return enumerator.Current;
			}
		}

		[RewriteAsync]
		internal static T SingleOrExceptionIfFirstIsNull<T>(this IEnumerable<T?> source)
			where T : struct
		{
			using (var enumerator = source.GetEnumeratorEx())
			{
				if (!enumerator.MoveNextEx() || enumerator.Current == null)
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}

				return enumerator.Current.Value;
			}
		}

		public static void WithEach<T>(this IEnumerable<T> source, Action<T> value)
		{
			using (var enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNextEx())
				{
					value(enumerator.Current);
				}
			}
		}

		public static void WithEach<T>(this IEnumerable<T> source, Func<T, bool> value)
		{
			using (var enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNextEx())
				{
					if (!value(enumerator.Current))
					{
						break;
					}
				}
			}
		}
		
		public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> source)
		{
			if (source == null)
			{
				return null;
			}

			var readonlyCollection = source as ReadOnlyCollection<T>;

			if (readonlyCollection != null)
			{
				return readonlyCollection;
			}

			var list = source as IList<T>;

			if (list != null)
			{
				return new ReadOnlyCollection<T>(list);
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var collection = source as ICollection<T>;

			var retval = collection == null ? new List<T>() : new List<T>(collection.Count);

			using (var enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					retval.Add(enumerator.Current);
				}
			}

			return new ReadOnlyCollection<T>(retval);
		}

		internal static Task<List<T>> ToListAsync<T>(this IEnumerable<T> source)
		{
			return source.ToListAsync(CancellationToken.None);
		}

		internal static async Task<List<T>> ToListAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			var queryable = source as ReusableQueryable<T>;

			if (queryable == null)
			{
				return source.ToList();
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var collection = source as ICollection<T>;

			var result = collection == null ? new List<T>() : new List<T>(collection.Count);

			using (var enumerator = queryable.GetAsyncEnumerator())
			{
				while (await enumerator.MoveNextAsync(cancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();

					result.Add(enumerator.Current);
				}
			}

			return result;
		}

		internal static Task<ReadOnlyCollection<T>> ToReadOnlyCollectionAsync<T>(this IEnumerable<T> source)
		{
			return source.ToReadOnlyCollectionAsync(CancellationToken.None);
		}

		internal static async Task<ReadOnlyCollection<T>> ToReadOnlyCollectionAsync<T>(this IEnumerable<T> source, CancellationToken cancellationToken)
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			var queryable = source as ReusableQueryable<T>;

			if (queryable != null)
			{
				return queryable.ToReadOnlyCollection();
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var readonlyCollection = source as ReadOnlyCollection<T>;

			if (readonlyCollection != null)
			{
				return readonlyCollection;
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var list = source as IList<T>;

			if (list != null)
			{
				return new ReadOnlyCollection<T>(list);
			}

			// ReSharper disable once SuspiciousTypeConversion.Global
			var collection = source as ICollection<T>;

			var retval = collection == null ? new List<T>() : new List<T>(collection.Count);

			using (var enumerator = source.GetAsyncEnumerator())
			{
				while (await enumerator.MoveNextAsync(cancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();

					retval.Add(enumerator.Current);
				}
			}

			return new ReadOnlyCollection<T>(retval);
		}

		internal static Task WithEachAsync<T>(this IEnumerable<T> source, Func<T, Task> value)
		{
			return source.WithEachAsync(value, CancellationToken.None);
		}

		internal static Task WithEachAsync<T>(this IEnumerable<T> queryable, Func<T, Task<bool>> value)
		{
			return queryable.WithEachAsync(value, CancellationToken.None);
		}

		internal static async Task WithEachAsync<T>(this IEnumerable<T> source, Func<T, Task<bool>> value, CancellationToken cancellationToken)
		{
			using (var enumerator = await source.GetEnumeratorExAsync())
			{
				while (await enumerator.MoveNextAsync(cancellationToken))
				{
					cancellationToken.ThrowIfCancellationRequested();

					if (!await value(enumerator.Current).ConfigureAwait(false))
					{
						break;
					}
				}
			}
		}

		internal static async Task WithEachAsync<T>(this IEnumerable<T> source, Func<T, Task> value, CancellationToken cancellationToken)
		{
			using (var enumerator = await source.GetEnumeratorExAsync())
			{
				while (await enumerator.MoveNextAsync(cancellationToken))
				{
					await value(enumerator.Current).ConfigureAwait(false);

					cancellationToken.ThrowIfCancellationRequested();
				}
			}
		}

		public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
		{
			return source.ToListAsync(CancellationToken.None);
		}

		public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
		{
			return ((IEnumerable<T>)source).ToListAsync(cancellationToken);
		}

		public static Task<ReadOnlyCollection<T>> ToReadOnlyCollectionAsync<T>(this IAsyncEnumerable<T> source)
		{
			return ((IEnumerable<T>)source).ToReadOnlyCollectionAsync();
		}

		public static Task<ReadOnlyCollection<T>> ToReadOnlyCollectionAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
		{
			return ((IEnumerable<T>)source).ToReadOnlyCollectionAsync(cancellationToken);
		}

		public static Task WithEachAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task> value)
		{
			return ((IEnumerable<T>)source).WithEachAsync(value);
		}

		public static Task WithEachAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> value)
		{
			return ((IEnumerable<T>)source).WithEachAsync(value);
		}

		public static Task WithEachAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> value, CancellationToken cancellationToken)
		{
			return ((IEnumerable<T>)source).WithEachAsync(value, cancellationToken);
		}

		public static Task WithEachAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task> value, CancellationToken cancellationToken)
		{
			return ((IEnumerable<T>)source).WithEachAsync(value, cancellationToken);
		}
	}
}
