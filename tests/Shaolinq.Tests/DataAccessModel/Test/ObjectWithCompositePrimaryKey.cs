// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

using Platform.Validation;

namespace Shaolinq.Tests.DataAccessModel.Test
{
	[DataAccessObject]
	public abstract class ObjectWithCompositePrimaryKey
		: DataAccessObject<long>
	{
		[AutoIncrement(false)]
		public abstract override long Id { get; set; }

		[PrimaryKey, PersistedMember, SizeConstraint(MaximumLength = 128)]
		public abstract string SecondaryKey { get; set; }

		[PersistedMember]
		public abstract string Name { get; set; }
	}
}