using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;

namespace UTAPI.Utils
{
    /// <summary>
    /// A custom contract resolver that allows properties to be ignored during serialization based on their names.
    /// </summary>
    public class IgnoreProperties : DefaultContractResolver
    {
        private readonly HashSet<string> ignoreProps;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreProperties"/> class with a set of property names to ignore.
        /// </summary>
        /// <param name="propNamesToIgnore">A collection of property names to ignore during serialization.</param>
        public IgnoreProperties(IEnumerable<string> propNamesToIgnore)
        {
            this.ignoreProps = new HashSet<string>(propNamesToIgnore);
        }

        /// <summary>
        /// Creates a property for serialization, but ignores it if it is in the list of properties to ignore.
        /// </summary>
        /// <param name="member">The member (property) to serialize.</param>
        /// <param name="memberSerialization">Specifies how the member is serialized.</param>
        /// <returns>A <see cref="JsonProperty"/> object representing the serialized property.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (this.ignoreProps.Contains(property.PropertyName))
            {
                property.ShouldSerialize = _ => false;  // Ignore the property during serialization
            }
            return property;
        }
    }
}
