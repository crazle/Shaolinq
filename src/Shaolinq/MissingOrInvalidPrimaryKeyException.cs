// Copyright (c) 2007-2016 Thong Nguyen (tumtumtum@gmail.com)
namespace Shaolinq
{
	public class MissingOrInvalidPrimaryKeyException
		: DataAccessException
	{
		public MissingOrInvalidPrimaryKeyException()
		{	
		}

		public MissingOrInvalidPrimaryKeyException(string message)
			: base(message, null)
		{
		}
	}
}
