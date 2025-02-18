using CK.Cris;
using CK.DeviceModel;

namespace CK.IO.DeviceModel.ByTopic;

/// <summary>
/// Cris command that will be dispatched to the targeted device hosts, based on the given topics.
/// </summary>
public interface ISetTopicColorCommand : ICommandDeviceTopics, ICommand<ISetTopicCommandResult>
{
    /// <summary>
    /// The desired <see cref="StandardColor"/> to turn targeted topics with.
    /// </summary>
    StandardColor Color { get; set; }
}
