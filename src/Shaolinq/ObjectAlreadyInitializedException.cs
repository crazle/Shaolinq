﻿using System;

namespace Shaolinq
{
	public class ObjectAlreadyInitializedException
		: Exception
	{
		public Object RelatedObject { get; private set; }

		public ObjectAlreadyInitializedException(object relatedObject)
		{
			this.RelatedObject = relatedObject;
		}
	}
}