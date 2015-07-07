using System.Collections.Generic;
using System.Linq;

namespace SuperGlue.EventStore.Data
{
    public class ProjectionBuilder
    {
        private const string FromStreamsTemplate = @"fromStreams([[[FromStreams]]]) 
                                                        .when({          
                                                            $any: function (s, e){
                                                                linkTo('[[ToStream]]', e);
                                                            }
                                                        })";

        public string BuildStreamProjection(IEnumerable<string> fromStreams, string toStream)
        {
            return FromStreamsTemplate
                .Replace("[[FromStreams]]", string.Join(", ", fromStreams.Select(x => string.Format("'{0}'", x))))
                .Replace("[[ToStream]]", toStream);
        }
    }
}