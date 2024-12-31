using CK.Cris;
using System.Collections.Generic;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ITurnOffMultipleTopicsCommand : ICommand<ISwitchMultipleTopicsCommandResult>
{
    IList<ITurnOffTopicCommand> Topics { get; }
}
