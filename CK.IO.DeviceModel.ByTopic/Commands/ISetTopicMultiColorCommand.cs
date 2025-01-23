using CK.Cris;
using CK.DeviceModel;

namespace CK.IO.DeviceModel.ByTopic
{
    public interface ISetTopicMultiColorCommand : ICommandDeviceTopics, ICommand<ISetTopicCommandResult>
    {
        IList<StandardColor> Colors { get; }
        TimeSpan TurnOffAfter { get; set; }
    }
}
