using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.DeviceModel.ByTopic.Tests.Helpers;
public class MessageHelper
{
    public static string TopicNotFound( string topic, string deviceHostName )
    => $"{topic} does not exist on {deviceHostName}";

    public static string TopicOn( string topic, string deviceHostName, StandardColor color )
    => $"{topic} turn on {deviceHostName} with {color.ToString( "G" )}";

    public static string TopicOff( string topic, string deviceHostName )
        => $"{topic} turn off {deviceHostName}";

    public static string TopicOnMultiColor( string topic, string deviceHostName, IList<StandardColor> colors )
        => $"{topic} turn on {deviceHostName} with {string.Join( ",", colors.Select( x => x.ToString( "G" ) ) )}";
}
