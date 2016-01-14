using System;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Json.Linq;

namespace SuperGlue.RavenDb
{
    public static class DocumentSessionExtensions
    {
        public static async Task StoreAndExpire(this IAsyncDocumentSession session, object item, DateTime expireAt)
        {
            await session.StoreAsync(item).ConfigureAwait(false);
            session.Advanced.GetMetadataFor(item)["Raven-Expiration-Date"] = new RavenJValue(expireAt);
        }
    }
}