using CK.Core;
using System.Collections.Generic;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ISwitchMultipleTopicsCommandResult : IPoco
{
    public IList<ISwitchTopicCommandResult> Results { get; }
}
