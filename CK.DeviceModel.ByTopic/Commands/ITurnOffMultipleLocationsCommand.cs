using CK.Cris;
using System.Collections.Generic;

namespace CK.DeviceModel.ByTopic.Commands;

public interface ITurnOffMultipleLocationsCommand : ICommand<ISwitchMultipleLocationsCommandResult>
{
    IList<ITurnOffLocationCommand> Locations { get; }
}
