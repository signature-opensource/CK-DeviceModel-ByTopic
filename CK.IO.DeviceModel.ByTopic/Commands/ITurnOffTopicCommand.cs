using CK.Cris;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ITurnOffTopicCommand : ICommand<ISwitchTopicCommandResult>, ICommandDeviceTopicTarget
{
}
