// Copyright (c) 2007-2015 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Linq;
using System.Reflection;
using Platform;
using Platform.Collections;
using Platform.Reflection;
using Platform.Validation;

namespace Shaolinq.Persistence
{
	public class PropertyDescriptor
	{
		public Type OwnerType { get; private set; }
		public bool IsPrimaryKey { get; private set; }
		public string PersistedName { get; }
		public string PersistedShortName { get; private set; }
		public PropertyInfo PropertyInfo { get; }
		public UniqueAttribute UniqueAttribute { get; }
		public TypeDescriptor DeclaringTypeDescriptor { get; }
		public PrimaryKeyAttribute PrimaryKeyAttribute { get; }
		public DefaultValueAttribute DefaultValueAttribute { get; private set; }
		public AutoIncrementAttribute AutoIncrementAttribute { get; }
		public ValueRequiredAttribute ValueRequiredAttribute { get; private set; }
		public BackReferenceAttribute BackReferenceAttribute { get; }
		public PersistedMemberAttribute PersistedMemberAttribute { get; }
		public ForeignObjectConstraintAttribute ForeignObjectConstraintAttribute { get; private set; }
		public IReadOnlyList<IndexAttribute> IndexAttributes { get; private set; }
		public ComputedMemberAttribute ComputedMemberAttribute { get; }
		public ComputedTextMemberAttribute ComputedTextMemberAttribute { get; }
		public RelatedDataAccessObjectsAttribute RelatedDataAccessObjectsAttribute { get; }
		public string PropertyName { get { return this.PropertyInfo.Name; } }
		public Type PropertyType { get { return this.PropertyInfo.PropertyType; } }
		public bool HasUniqueAttribute { get { return this.UniqueAttribute != null; } }
		public bool IsBackReferenceProperty { get { return this.BackReferenceAttribute != null; } }
		public bool IsComputedMember { get { return this.ComputedMemberAttribute != null; } }
		public bool IsComputedTextMember { get { return this.ComputedTextMemberAttribute != null; } }
		public bool IsRelatedDataAccessObjectsProperty { get { return this.RelatedDataAccessObjectsAttribute != null; } }
		public bool IsAutoIncrement { get { return this.AutoIncrementAttribute != null && this.AutoIncrementAttribute.AutoIncrement; } }
		public bool IsPropertyThatIsCreatedOnTheServerSide { get { return this.IsAutoIncrement && this.PropertyType.IsIntegerType(true); } }

		public static bool IsPropertyPrimaryKey(PropertyInfo propertyInfo)
		{
			var value =  propertyInfo.GetFirstCustomAttribute<PrimaryKeyAttribute>(true);

			return value != null && value.IsPrimaryKey;
		}

		public PropertyDescriptor(TypeDescriptor declaringTypeDescriptor, Type ownerType, PropertyInfo propertyInfo)
		{
			this.OwnerType = ownerType;
			this.PropertyInfo = propertyInfo;
			this.DeclaringTypeDescriptor = declaringTypeDescriptor;

			this.ValueRequiredAttribute = propertyInfo.GetFirstCustomAttribute<ValueRequiredAttribute>(true);
			this.DefaultValueAttribute = propertyInfo.GetFirstCustomAttribute<DefaultValueAttribute>(true);
			this.BackReferenceAttribute = propertyInfo.GetFirstCustomAttribute<BackReferenceAttribute>(true);
			this.RelatedDataAccessObjectsAttribute = propertyInfo.GetFirstCustomAttribute<RelatedDataAccessObjectsAttribute>(true);
			this.PersistedMemberAttribute = propertyInfo.GetFirstCustomAttribute<PersistedMemberAttribute>(true);
			this.ComputedMemberAttribute = propertyInfo.GetFirstCustomAttribute<ComputedMemberAttribute>(true);
			this.ComputedTextMemberAttribute = propertyInfo.GetFirstCustomAttribute<ComputedTextMemberAttribute>(true);
			this.ForeignObjectConstraintAttribute = propertyInfo.GetFirstCustomAttribute<ForeignObjectConstraintAttribute>(true);

			if (this.PropertyType.IsIntegerType(true) || this.PropertyType.GetUnwrappedNullableType() == typeof(Guid))
			{
				this.AutoIncrementAttribute = propertyInfo.GetFirstCustomAttribute<AutoIncrementAttribute>(true);
			}

			this.PrimaryKeyAttribute = this.PropertyInfo.GetFirstCustomAttribute<PrimaryKeyAttribute>(true);
			this.IsPrimaryKey = this.PrimaryKeyAttribute != null && this.PrimaryKeyAttribute.IsPrimaryKey;

			if (this.PersistedMemberAttribute != null)
			{
				this.PersistedName = this.PersistedMemberAttribute.GetName(this.PropertyInfo, declaringTypeDescriptor);
				this.PersistedShortName = this.PersistedMemberAttribute.GetShortName(this.PropertyInfo, this.DeclaringTypeDescriptor);
			}
			else if (this.BackReferenceAttribute != null)
			{
				this.PersistedName = this.BackReferenceAttribute.GetName(this.PropertyInfo, declaringTypeDescriptor);
				this.PersistedShortName = this.PersistedName;
			}
			else if (this.RelatedDataAccessObjectsAttribute != null)
			{
				this.PersistedName = propertyInfo.Name;
				this.PersistedShortName = propertyInfo.Name;
			}

			this.IndexAttributes = new ReadOnlyList<IndexAttribute>(this.PropertyInfo.GetCustomAttributes(typeof(IndexAttribute), true).OfType<IndexAttribute>().ToList());
			this.UniqueAttribute = this.PropertyInfo.GetFirstCustomAttribute<UniqueAttribute>(true);
		}

		public override int GetHashCode()
		{
			return this.PropertyInfo.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var value = obj as PropertyDescriptor;

			return value != null && value.PropertyInfo == this.PropertyInfo;
		}

		public override string ToString()
		{
			return string.Format("{0}.{1} PropertyDescriptor", this.DeclaringTypeDescriptor.TypeName, this.PropertyName);
		}
	}
}
