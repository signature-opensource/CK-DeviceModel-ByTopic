using CK.Cris;

namespace CK.IO.DeviceModel.ByTopic.Commands;

public interface ITurnOnTopicCommand : ICommand<ISwitchTopicCommandResult>, ICommandDeviceTopicTarget
{
    List<ColorTopic> Colors { get; set; }
    TimeSpan TurnOfAfter { get; set; }
}
