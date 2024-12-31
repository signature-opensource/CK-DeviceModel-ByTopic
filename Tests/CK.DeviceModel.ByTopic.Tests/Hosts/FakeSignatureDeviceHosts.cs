using CK.Core;
using CK.Cris.DeviceModel;
using CK.IO.DeviceModel;
using CK.IO.DeviceModel.ByTopic.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.Tests.Hosts;

public class FakeSignatureDeviceHosts : IAutoService, ITopicTargetAwareDeviceHost
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
