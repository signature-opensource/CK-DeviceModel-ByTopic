using CK.Cris;
using CK.DeviceModel;

namespace CK.IO.DeviceModel.ByTopic
{
    public interface ISetTopicMultiColorCommand : ICommandDeviceTopics, ICommand<ISetTopicCommandResult>
    {
        List<StandardColor> Colors { get; set; }
        TimeSpan TurnOffAfter { get; set; }
    }
}
