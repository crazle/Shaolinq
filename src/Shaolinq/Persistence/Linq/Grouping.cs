// Copyright (c) 2007-2014 Thong Nguyen (tumtumtum@gmail.com)

﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shaolinq.Persistence.Linq
{
	public class Grouping<K, V>
		: IGrouping<K, V>
	{
		public K Key { get; private set; }
		public IEnumerable<V> Group { get; private set; }

		public Grouping(K key, IEnumerable<V> group)
		{
			this.Key = key;
			this.Group = group;
		}

		public Grouping(K key, ObjectProjector projector)
		{
			this.Key = key;
		}

		public IEnumerator<V> GetEnumerator()
		{
			return this.Group.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Group.GetEnumerator();
		}
	}
}
