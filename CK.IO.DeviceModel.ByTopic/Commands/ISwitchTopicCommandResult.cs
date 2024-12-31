using CK.Core;
using System.Collections.Generic;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ISwitchTopicCommandResult : IPoco
{
    public string Topic { get; set; }
    public Dictionary<string, bool> ResultByDeviceName { get; set; }
}
