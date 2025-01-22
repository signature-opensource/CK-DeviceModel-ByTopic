using CK.Core;
using CK.Cris.DeviceModel;
using CK.DeviceModel.ByTopic.Tests.Helpers;
using CK.IO.DeviceModel;
using CK.IO.DeviceModel.ByTopic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.Tests.Hosts;

public class FakeLEDStripHosts : IAutoService, ITopicAwareDeviceHost
{
    public string DeviceHostName { get; set; }

    public List<string> Topics { get; set; }

    public FakeLEDStripHosts()
    {
        DeviceHostName = nameof( FakeLEDStripHosts );
        Topics = new List<string>()
        {
            "Test",
            "Test-1",
            "Test-FakeLEDStrip",
        };
    }

    public ValueTask HandleAsync( IActivityMonitor monitor, UserMessageCollector userMessageCollector, ICommandDeviceTopics cmd )
    {
        var topics = cmd.Topics.ToList();
        foreach( var topic in cmd.Topics )
        {
            var topicName = topic.Split( "/" ).Last();
            if( !Topics.Contains( topicName ) )
            {
                userMessageCollector.Error( MessageHelper.TopicNotFound( topic, DeviceHostName ) );
                topics.Remove( topic );
            }
        }

        if( userMessageCollector.ErrorCount == cmd.Topics.Count )
        {
            return ValueTask.CompletedTask;
        }

        if( cmd is ISetTopicColorCommand )
        {
            return ValueTask.CompletedTask;
        }
        else if( cmd is ISetTopicMultiColorCommand )
        {
            return ValueTask.CompletedTask;
        }
        else
        {
            userMessageCollector.Error( $" Unsupported command on {DeviceHostName} " );
            return ValueTask.CompletedTask;
        }

    }
}
