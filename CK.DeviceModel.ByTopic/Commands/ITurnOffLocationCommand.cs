using CK.Cris;
using CK.DeviceModel.ByTopic.IO.Commands;

namespace CK.DeviceModel.ByTopic.Commands;

public interface ITurnOffLocationCommand : ICommand<ISwitchLocationCommandResult>, ICommandDeviceTopicTarget
{
}
