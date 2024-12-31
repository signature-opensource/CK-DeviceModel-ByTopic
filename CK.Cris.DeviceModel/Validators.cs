using CK.Core;
using CK.IO.DeviceModel;
using System.Text.RegularExpressions;

namespace CK.Cris.DeviceModel;

public sealed partial class Validators : IAutoService
{

    [IncomingValidator]
    public void Normalize( UserMessageCollector collector, ICommandDeviceTopicTarget cmd )
    {
        if( cmd.DeviceFullName != null && !DeviceFullNameValidator().IsMatch( cmd.DeviceFullName ) )
        {
            collector.Error( $"Invalid DeviceFullName." );
        }
        if( !string.IsNullOrEmpty( cmd.Topic ) && cmd.Topic[0] == '/' )
        {
            collector.Warn( "Topic should not start with a '/'." );
            cmd.Topic = cmd.Topic.Substring( 1 );
        }
    }

    [GeneratedRegex( @"\w+(/\w+)?", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant )]
    private static partial Regex DeviceFullNameValidator();

}
