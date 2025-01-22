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
    private readonly ScopedUserMessageCollector _collector;
    readonly PocoDirectory _pocoDirectory;

    public ByTopicCommandHandler( IEnumerable<ITopicAwareDeviceHost> hosts, ScopedUserMessageCollector collector, PocoDirectory pocoDirectory )
    {
        _hosts = hosts;
        _collector = collector;
        _pocoDirectory = pocoDirectory;
    }

    IEnumerable<ITopicAwareDeviceHost> ForDeviceFullName( string? deviceHostName )
    {
        if( string.IsNullOrWhiteSpace( deviceHostName ) ) return _hosts;
        if( deviceHostName == "*" ) return _hosts;

        return _hosts.Where( x => x.DeviceHostName.StartsWith( deviceHostName ) );
    }

    [CommandHandler]
    public async Task<ISetTopicCommandResult> HandleTurnOnTopicCommandAsync( IActivityMonitor monitor, ISetTopicColorCommand cmd )
    {
        await HandleDeviceTopicCommandAsync( monitor, cmd );

        //TODO: See why we cant use CreateResult
        return _pocoDirectory.Create<ISetTopicCommandResult>( r =>
        {
            r.Success = _collector.ErrorCount == 0;
            r.UserMessages.AddRange( _collector.UserMessages );
        } );
    }

    [CommandHandler]
    public async Task<ISetTopicCommandResult> HandleTurnOnMultipleTopicsCommandAsync( IActivityMonitor monitor, ISetTopicMultiColorCommand cmd )
    {
        await HandleDeviceTopicCommandAsync( monitor, cmd );

        //TODO: See why we cant use CreateResult
        return _pocoDirectory.Create<ISetTopicCommandResult>( r =>
        {
            r.Success = _collector.ErrorCount == 0;
            r.UserMessages.AddRange( _collector.UserMessages );
        } );

    }

    private async Task HandleDeviceTopicCommandAsync( IActivityMonitor monitor, ICommandDeviceTopics cmd )
    {
        foreach( var topic in cmd.Topics )
        {
            var deviceHostName = topic.Split( "/" )[0];
            var targets = ForDeviceFullName( deviceHostName );
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
