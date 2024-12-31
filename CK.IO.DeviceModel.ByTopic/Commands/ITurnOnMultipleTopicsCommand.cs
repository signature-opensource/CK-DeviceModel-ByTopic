using CK.Cris;
using System.Collections.Generic;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ITurnOnMultipleTopicsCommand : ICommand<ISwitchMultipleTopicsCommandResult>
{
    IList<ITurnOnTopicCommand> Topics { get; }
}
