using CK.Core;
using CK.Cris;
using CK.Cris.DeviceModel;
using CK.IO.DeviceModel;
using CK.IO.DeviceModel.ByTopic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic;

public class ByTopicCommandHandler : IAutoService
{
    readonly IEnumerable<ITopicAwareDeviceHost> _hosts;
    readonly ScopedUserMessageCollector _collector;

    public ByTopicCommandHandler( IEnumerable<ITopicAwareDeviceHost> hosts, ScopedUserMessageCollector collector )
    {
        _hosts = hosts;
        _collector = collector;
    }

    [CommandHandler]
    public async Task<ISetTopicCommandResult> HandleTurnOnTopicCommandAsync( IActivityMonitor monitor, ISetTopicColorCommand cmd )
    {
        await HandleDeviceTopicCommandAsync( monitor, cmd );
        return cmd.CreateResult<ISetTopicCommandResult>( c => c.SetUserMessages( _collector ) );
    }

    [IncomingValidator]
    public void ValidateMultipleTopicsCommand( IActivityMonitor m, UserMessageCollector collector, ISetTopicMultiColorCommand cmd )
    {
        using( m.OpenInfo( "Validating ISetTopicMultiColorCommand." ) )
        {
            if( !cmd.Colors.Any() )
            {
                collector.Error( "Colors collection must not be empty." );
            }
        }
    }

    [CommandHandler]
    public async Task<ISetTopicCommandResult> HandleTurnOnMultipleTopicsCommandAsync( IActivityMonitor monitor, ISetTopicMultiColorCommand cmd )
    {
        await HandleDeviceTopicCommandAsync( monitor, cmd );
        return cmd.CreateResult<ISetTopicCommandResult>( c => c.SetUserMessages( _collector ) );
    }

    async Task HandleDeviceTopicCommandAsync( IActivityMonitor monitor, ICommandDeviceTopics cmd )
    {
        foreach( var topic in cmd.Topics )
        {
            var deviceHostName = topic.Split( "/" )[0];
            var targets = _hosts;
            if( !string.IsNullOrWhiteSpace( deviceHostName ) && deviceHostName != "*" )
            {
                targets =_hosts.Where( x => x.DeviceHostName.StartsWith( deviceHostName ) );
            }

            if( targets.Any() )
            {
                foreach( var host in targets )
                {
                    await host.HandleAsync( monitor, _collector, cmd ).ConfigureAwait( false );
                }
            }
            else
            {
                _collector.Error( $"{deviceHostName} not found in hosts" );
            }
        }
    }
}
