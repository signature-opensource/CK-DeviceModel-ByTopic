using CK.Core;
using CK.Cris;
using CK.Cris.DeviceModel;
using CK.IO.DeviceModel.ByTopic.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.CommandHandlers;

public class ByTopicCommandHandler : IAutoService
{
    readonly IEnumerable<ITopicTargetAwareDeviceHost> _hosts;

    public ByTopicCommandHandler( IEnumerable<ITopicTargetAwareDeviceHost> hosts )
    {
        _hosts = hosts;
    }

    IEnumerable<ITopicTargetAwareDeviceHost> ForDeviceFullName( string? deviceFullName )
    {
        if( string.IsNullOrWhiteSpace( deviceFullName ) ) return _hosts;

        var deviceHostName = deviceFullName.Split( "/" )[0];
        return _hosts.Where( x => x.DeviceHostName.StartsWith( deviceHostName ) );
    }

    [CommandHandler]
    public async Task<ISwitchTopicCommandResult> HandleTurnOnTopicCommandAsync( IActivityMonitor monitor, ITurnOnTopicCommand cmd )
    {
        var targets = ForDeviceFullName( cmd.DeviceFullName );
        var resultByDeviceName = new Dictionary<string, bool>();
        foreach( var host in targets )
        {
            var result = await host.HandleAsync( monitor, cmd ).ConfigureAwait( false );
            resultByDeviceName.TryAdd( host.DeviceHostName, result );
        }

        return cmd.CreateResult( r =>
         {
             r.Topic = cmd.Topic;
             r.ResultByDeviceName = resultByDeviceName;
         } );
    }

    [CommandHandler]
    public async Task<ISwitchMultipleTopicsCommandResult> HandleTurnOnMultipleTopicsCommandAsync( IActivityMonitor monitor, ITurnOnMultipleTopicsCommand cmd )
    {
        var turnOnTopicCommandResults = new List<ISwitchTopicCommandResult>();
        foreach( var c in cmd.Topics )
        {
            turnOnTopicCommandResults.Add( await HandleTurnOnTopicCommandAsync( monitor, c ));
        }

        return cmd.CreateResult( r =>
        {
            r.Results.AddRange(turnOnTopicCommandResults);
        } );
    }


    [CommandHandler]
    public async Task<ISwitchTopicCommandResult> HandleTurnOffTopicCommandAsync( IActivityMonitor monitor, ITurnOffTopicCommand cmd )
    {
        var targets = ForDeviceFullName( cmd.DeviceFullName );
        var resultByDeviceName = new Dictionary<string, bool>();
        foreach( var host in targets )
        {
            var result = await host.HandleAsync( monitor, cmd ).ConfigureAwait( false );
            resultByDeviceName.TryAdd( host.DeviceHostName, result );
        }

        return cmd.CreateResult( r =>
        {
            r.Topic = cmd.Topic;
            r.ResultByDeviceName = resultByDeviceName;
        } );
    }

    [CommandHandler]
    public async Task<ISwitchMultipleTopicsCommandResult> HandleTurnOffMultipleTopicsCommandAsync( IActivityMonitor monitor, ITurnOffMultipleTopicsCommand cmd )
    {
        var turnOffTopicCommandResults = new List<ISwitchTopicCommandResult>();
        foreach( var c in cmd.Topics )
        {
            turnOffTopicCommandResults.Add( await HandleTurnOffTopicCommandAsync( monitor, c ) );
        }

        return cmd.CreateResult( r =>
        {
            r.Results.AddRange(turnOffTopicCommandResults);
        } );
    }

}
