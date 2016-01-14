using System.Collections;
using System.Collections.Generic;
using SuperGlue.Web.Output;

namespace SuperGlue.Web.Assets
{
    public class AssetPlanKey : IEnumerable<string>
    {
        public MimeType MimeType { get; }

        public IEnumerable<string> Names { get; }

        public AssetPlanKey(MimeType mimeType, IEnumerable<string> names)
        {
            MimeType = mimeType;
            Names = names;
        }

        public bool Equals(AssetPlanKey other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (Equals(other.MimeType, MimeType))
                return other.Names.IsEqualTo(Names);

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            
            if (ReferenceEquals(this, obj))
                return true;
            
            if (obj.GetType() != typeof(AssetPlanKey))
                return false;
            
            return Equals((AssetPlanKey)obj);
        }

        public override int GetHashCode()
        {
            return (MimeType?.GetHashCode() ?? 0) * 397 ^ (Names?.Join("*").GetHashCode() ?? 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static AssetPlanKey For(MimeType mimeType, params string[] names)
        {
            return new AssetPlanKey(mimeType, names);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Names.GetEnumerator();
        }

        public override string ToString()
        {
            return $"MimeType: {MimeType}, Names: {Names.Join(", ")}";
        }
    }
}