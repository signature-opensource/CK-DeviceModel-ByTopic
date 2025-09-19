using CK.Cris;

namespace CK.IO.DeviceModel.ByTopic;

/// <summary>
/// Cris command that will be dispatched to the targeted device hosts, based on the given topics.
/// </summary>
public interface ISetTopicMultiColorCommand : ICommand<ISetTopicCommandResult>, ICommandDeviceTopics
{
    /// <summary>
    /// The collection of the desired <see cref="StandardColor"/> to turn targeted topics with.
    /// </summary>
    IList<StandardColor> Colors { get; }
}
