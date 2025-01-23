using CK.Core;
using CK.Cris.DeviceModel;
using CK.DeviceModel.ByTopic.Tests.Helpers;
using CK.IO.DeviceModel;
using CK.IO.DeviceModel.ByTopic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.Tests.Hosts;

public class FakeSignatureDeviceHosts : IAutoService, ITopicAwareDeviceHost
{
    public string DeviceHostName { get; set; }

    public List<string> Topics { get; set; }

    public FakeSignatureDeviceHosts()
    {
        DeviceHostName = nameof( FakeSignatureDeviceHosts );
        Topics = new List<string>()
        {
            "Test",
            "Test-1",
            "Test-FakeSignatureDevice",
        };
    }

    public ValueTask HandleAsync( IActivityMonitor monitor, UserMessageCollector userMessageCollector, ICommandDeviceTopics cmd )
    {
        var topics = cmd.Topics.ToList();
        var localUserMessageCollector = new List<string>();
        foreach( var topic in cmd.Topics )
        {
            var topicName = topic.Split( "/" ).Last();
            if( !Topics.Contains( topicName ) )
            {
                localUserMessageCollector.Add( MessageHelper.TopicNotFound( topic, DeviceHostName ) );
                userMessageCollector.Error( MessageHelper.TopicNotFound( topic, DeviceHostName ) );
                topics.Remove( topic );
            }
        }

        if( localUserMessageCollector.Count == cmd.Topics.Count )
        {
            return ValueTask.CompletedTask;
        }

        if( cmd is ISetTopicColorCommand setTopicColorCommand )
        {
            foreach( var item in topics )
            {
                if( setTopicColorCommand.Color == StandardColor.Off )
                {
                    userMessageCollector.Info( MessageHelper.TopicOff( item, DeviceHostName ) );
                }
                else
                {
                    userMessageCollector.Info( MessageHelper.TopicOn( item, DeviceHostName, setTopicColorCommand.Color ) );
                }
            }
            return ValueTask.CompletedTask;
        }
        else if( cmd is ISetTopicMultiColorCommand setTopicMultiColorCommand )
        {
            foreach( var item in topics )
            {
                if( setTopicMultiColorCommand.Colors.All( x => x == StandardColor.Off ) )
                {
                    userMessageCollector.Info( MessageHelper.TopicOff( item, DeviceHostName ) );

                }
                else
                {
                    userMessageCollector.Info( MessageHelper.TopicOnMultiColor( item, DeviceHostName, setTopicMultiColorCommand.Colors ) );
                }
            }
            return ValueTask.CompletedTask;
        }
        else
        {
            userMessageCollector.Error( $" Unsupported command on {DeviceHostName} " );
            return ValueTask.CompletedTask;
        }

    }
}
