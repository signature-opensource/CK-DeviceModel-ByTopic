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
}
