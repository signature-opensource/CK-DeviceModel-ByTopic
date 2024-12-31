using CK.Core;
using CK.Cris.DeviceModel;
using CK.IO.DeviceModel;
using CK.IO.DeviceModel.ByTopic.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.Tests.Hosts;

public class FakeLEDStripHosts : IAutoService, ITopicTargetAwareDeviceHost
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

    public ValueTask<bool> HandleAsync( IActivityMonitor monitor, ICommandDeviceTopicTarget cmd )
    {
        if( !Topics.Contains( cmd.Topic ) )
        {
            return ValueTask.FromResult( false );
        }

        if( cmd is ITurnOffTopicCommand )
        {
            return ValueTask.FromResult( true );
        }
        else if( cmd is ITurnOnTopicCommand )
        {
            return ValueTask.FromResult( true );
        }
        else
        {
            return ValueTask.FromResult( false );
        }
    }
}
