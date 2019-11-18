﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a property type.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
    public class PropertyType : EntityBase, IPropertyType, IEquatable<PropertyType>
    {
        private readonly bool _forceValueStorageType;
        private string _name;
        private string _alias;
        private string _description;
        private int _dataTypeId;
        private Guid _dataTypeKey;
        private Lazy<int> _propertyGroupId;
        private string _propertyEditorAlias;
        private ValueStorageType _valueStorageType;
        private bool _mandatory;
        private int _sortOrder;
        private string _validationRegExp;
        private ContentVariation _variations;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(IDataType dataType)
        {
            if (dataType == null) throw new ArgumentNullException(nameof(dataType));

            if (dataType.HasIdentity)
                _dataTypeId = dataType.Id;

            _propertyEditorAlias = dataType.EditorAlias;
            _valueStorageType = dataType.DatabaseType;
            _variations = ContentVariation.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(IDataType dataType, string propertyTypeAlias)
            : this(dataType)
        {
            _alias = SanitizeAlias(propertyTypeAlias);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType)
            : this(propertyEditorAlias, valueStorageType, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        public PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, string propertyTypeAlias)
            : this(propertyEditorAlias, valueStorageType, false, propertyTypeAlias)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyType"/> class.
        /// </summary>
        /// <remarks>Set <paramref name="forceValueStorageType"/> to true to force the value storage type. Values assigned to
        /// the property, eg from the underlying datatype, will be ignored.</remarks>
        internal PropertyType(string propertyEditorAlias, ValueStorageType valueStorageType, bool forceValueStorageType, string propertyTypeAlias = null)
        {
            _propertyEditorAlias = propertyEditorAlias;
            _valueStorageType = valueStorageType;
            _forceValueStorageType = forceValueStorageType;
            _alias = propertyTypeAlias == null ? null : SanitizeAlias(propertyTypeAlias);
            _variations = ContentVariation.Nothing;
        }

        /// <summary>
        /// Gets a value indicating whether the content type owning this property type is publishing.
        /// </summary>
        /// <remarks>
        /// <para>A publishing content type supports draft and published values for properties.
        /// It is possible to retrieve either the draft (default) or published value of a property.
        /// Setting the value always sets the draft value, which then needs to be published.</para>
        /// <para>A non-publishing content type only supports one value for properties. Getting
        /// the draft or published value of a property returns the same thing, and publishing
        /// a value property has no effect.</para>
        /// <para>When true, getting the property value returns the edited value by default, but
        /// it is possible to get the published value using the appropriate 'published' method
        /// parameter.</para>
        /// <para>When false, getting the property value always return the edited value,
        /// regardless of the 'published' method parameter.</para>
        /// </remarks>
        public bool SupportsPublishing { get; internal set; }

        /// <inheritdoc />
        [DataMember]
        public string Name
        {
            get => _name;
            set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
        }

        /// <inheritdoc />
        [DataMember]
        public virtual string Alias
        {
            get => _alias;
            set => SetPropertyValueAndDetectChanges(SanitizeAlias(value), ref _alias, nameof(Alias));
        }

        /// <inheritdoc />
        [DataMember]
        public string Description
        {
            get => _description;
            set => SetPropertyValueAndDetectChanges(value, ref _description, nameof(Description));
        }

        /// <inheritdoc />
        [DataMember]
        public int DataTypeId
        {
            get => _dataTypeId;
            set => SetPropertyValueAndDetectChanges(value, ref _dataTypeId, nameof(DataTypeId));
        }

        [DataMember]
        public Guid DataTypeKey
        {
            get => _dataTypeKey;
            set => SetPropertyValueAndDetectChanges(value, ref _dataTypeKey, nameof(DataTypeKey));
        }

        /// <inheritdoc />
        [DataMember]
        public string PropertyEditorAlias
        {
            get => _propertyEditorAlias;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyEditorAlias, nameof(PropertyEditorAlias));
        }

        /// <inheritdoc />
        [DataMember]
        public ValueStorageType ValueStorageType
        {
            get => _valueStorageType;
            set
            {
                if (_forceValueStorageType) return; // ignore changes
                SetPropertyValueAndDetectChanges(value, ref _valueStorageType, nameof(ValueStorageType));
            }
        }

        /// <inheritdoc />
        [DataMember]
        public Lazy<int> PropertyGroupId
        {
            get => _propertyGroupId;
            set => SetPropertyValueAndDetectChanges(value, ref _propertyGroupId, nameof(PropertyGroupId));
        }


        /// <inheritdoc />
        [DataMember]
        public bool Mandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, nameof(Mandatory));
        }


        /// <inheritdoc />
        [DataMember]
        public int SortOrder
        {
            get => _sortOrder;
            set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
        }

        /// <inheritdoc />
        [DataMember]
        public string ValidationRegExp
        {
            get => _validationRegExp;
            set => SetPropertyValueAndDetectChanges(value, ref _validationRegExp, nameof(ValidationRegExp));
        }

        /// <inheritdoc />
        public ContentVariation Variations
        {
            get => _variations;
            set => SetPropertyValueAndDetectChanges(value, ref _variations, nameof(Variations));
        }

        /// <inheritdoc />
        public bool SupportsVariation(string culture, string segment, bool wildcards = false)
        {
            // exact validation: cannot accept a 'null' culture if the property type varies
            //  by culture, and likewise for segment
            // wildcard validation: can accept a '*' culture or segment
            return Variations.ValidateVariation(culture, segment, true, wildcards, false);
        }

        /// <summary>
        /// Creates a new property of this property type.
        /// </summary>
        public Property CreateProperty()
        {
            return new Property(this);
        }

        /// <summary>
        /// Determines whether a value is of the expected type for this property type.
        /// </summary>
        /// <remarks>
        /// <para>If the value is of the expected type, it can be directly assigned to the property.
        /// Otherwise, some conversion is required.</para>
        /// </remarks>
        private bool IsOfExpectedPropertyType(object value)
        {
            // null values are assumed to be ok
            if (value == null)
                return true;

            // check if the type of the value matches the type from the DataType/PropertyEditor
            // then it can be directly assigned, anything else requires conversion
            var valueType = value.GetType();
            switch (ValueStorageType)
            {
                case ValueStorageType.Integer:
                    return valueType == typeof(int);
                case ValueStorageType.Decimal:
                    return valueType == typeof(decimal);
                case ValueStorageType.Date:
                    return valueType == typeof(DateTime);
                case ValueStorageType.Nvarchar:
                    return valueType == typeof(string);
                case ValueStorageType.Ntext:
                    return valueType == typeof(string);
                default:
                    throw new NotSupportedException($"Not supported storage type \"{ValueStorageType}\".");
            }
        }

        /// <inheritdoc />
        public object ConvertAssignedValue(object value) => TryConvertAssignedValue(value, true, out var converted) ? converted : null;

        /// <summary>
        /// Tries to convert a value assigned to a property.
        /// </summary>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        private bool TryConvertAssignedValue(object value, bool throwOnError, out object converted)
        {
            var isOfExpectedType = IsOfExpectedPropertyType(value);
            if (isOfExpectedType)
            {
                converted = value;
                return true;
            }

            // isOfExpectedType is true if value is null - so if false, value is *not* null
            // "garbage-in", accept what we can & convert
            // throw only if conversion is not possible

            var s = value.ToString();
            converted = null;

            switch (ValueStorageType)
            {
                case ValueStorageType.Nvarchar:
                case ValueStorageType.Ntext:
                {
                    converted = s;
                    return true;
                }

                case ValueStorageType.Integer:
                    if (s.IsNullOrWhiteSpace())
                        return true; // assume empty means null
                    var convInt = value.TryConvertTo<int>();
                    if (convInt)
                    {
                        converted = convInt.Result;
                        return true;
                    }

                    if (throwOnError)
                        ThrowTypeException(value, typeof(int), Alias);
                    return false;

                case ValueStorageType.Decimal:
                    if (s.IsNullOrWhiteSpace())
                        return true; // assume empty means null
                    var convDecimal = value.TryConvertTo<decimal>();
                    if (convDecimal)
                    {
                        // need to normalize the value (change the scaling factor and remove trailing zeros)
                        // because the underlying database is going to mess with the scaling factor anyways.
                        converted = convDecimal.Result.Normalize();
                        return true;
                    }

                    if (throwOnError)
                        ThrowTypeException(value, typeof(decimal), Alias);
                    return false;

                case ValueStorageType.Date:
                    if (s.IsNullOrWhiteSpace())
                        return true; // assume empty means null
                    var convDateTime = value.TryConvertTo<DateTime>();
                    if (convDateTime)
                    {
                        converted = convDateTime.Result;
                        return true;
                    }

                    if (throwOnError)
                        ThrowTypeException(value, typeof(DateTime), Alias);
                    return false;

                default:
                    throw new NotSupportedException($"Not supported storage type \"{ValueStorageType}\".");
            }
        }

        private static void ThrowTypeException(object value, Type expected, string alias)
        {
            throw new InvalidOperationException($"Cannot assign value \"{value}\" of type \"{value.GetType()}\" to property \"{alias}\" expecting type \"{expected}\".");
        }


        /// <summary>
        /// Sanitizes a property type alias.
        /// </summary>
        private static string SanitizeAlias(string value)
        {
            //NOTE: WE are doing this because we don't want to do a ToSafeAlias when the alias is the special case of
            // being prefixed with Constants.PropertyEditors.InternalGenericPropertiesPrefix
            // which is used internally

            return value.StartsWith(Constants.PropertyEditors.InternalGenericPropertiesPrefix)
                ? value
                : value.ToCleanString(CleanStringType.Alias | CleanStringType.UmbracoCase);
        }

        /// <inheritdoc />
        public bool Equals(PropertyType other)
        {
            return other != null && (base.Equals(other) || Alias.InvariantEquals(other.Alias));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            //Get hash code for the Name field if it is not null.
            int baseHash = base.GetHashCode();

            //Get hash code for the Alias field.
            int hashAlias = Alias.ToLowerInvariant().GetHashCode();

            //Calculate the hash code for the product.
            return baseHash ^ hashAlias;
        }

        /// <inheritdoc />
        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (PropertyType) clone;

            //need to manually assign the Lazy value as it will not be automatically mapped
            if (PropertyGroupId != null)
            {
                clonedEntity._propertyGroupId = new Lazy<int>(() => PropertyGroupId.Value);
            }
        }
    }
}
