using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace DevSubmarine.LukeDictionary.Database.Conventions
{
    //http://www.codewrecks.com/blog/index.php/2016/04/15/change-how-mongodb-c-driver-serialize-guid-in-new-driver-version/
    //https://stackoverflow.com/questions/62297194/how-can-i-tell-the-mongodb-c-sharp-driver-to-store-all-nullableguids-in-string

    /// <summary>
    /// A convention that allows you to set the serialization representation of guid to a simple string
    /// </summary>
    public class GuidAsStringRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// The member map.
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == typeof(Guid))
                memberMap.SetSerializer(new GuidSerializer(BsonType.String));
            else if (memberMap.MemberType == typeof(Guid?))
                memberMap.SetSerializer(new NullableSerializer<Guid>(new GuidSerializer(BsonType.String)));
        }
    }
}
