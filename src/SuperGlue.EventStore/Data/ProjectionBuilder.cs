using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public string BuildStreamProjection(IEnumerable<string> fromStreams, string toStream, IEnumerable<EventMap> eventMaps)
        {
            var projectionBuilder = new StringBuilder();

            projectionBuilder.AppendLine(string.Format("fromStreams([{0}])", string.Join(", ", fromStreams.Select(x => string.Format("'{0}'", x)))));
            projectionBuilder.AppendLine(".when({");

            foreach (var eventMap in eventMaps)
            {
                projectionBuilder.AppendLine(string.Format("{0}: function(s, e){1}", eventMap.EventName, "{"));

                projectionBuilder.AppendLine(string.IsNullOrEmpty(eventMap.PartitionKeyLocation)
                    ? string.Format("linkTo('{0}', e);", toStream)
                    : string.Format("linkTo('{0}', e, {2}'PartitionKey': 'e.data.{1}'{3});", toStream, eventMap.PartitionKeyLocation, "{", "}"));

                projectionBuilder.AppendLine("},");
            }

            projectionBuilder.AppendLine("})");

            return projectionBuilder.ToString();
        }

        public class EventMap
        {
            public EventMap(string eventName, string partitionKeyLocation = null)
            {
                EventName = eventName;
                PartitionKeyLocation = partitionKeyLocation;
            }

            public string EventName { get; private set; }
            public string PartitionKeyLocation { get; private set; }
        }
    }
}