using CK.Cris;
using CK.DeviceModel;

namespace CK.IO.DeviceModel.ByTopic
{
    public interface ISetTopicColorCommand : ICommandDeviceTopics, ICommand<ISetTopicCommandResult>
    {
        StandardColor Color { get; set; }
    }

}
