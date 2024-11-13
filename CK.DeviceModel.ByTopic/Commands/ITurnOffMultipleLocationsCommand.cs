using CK.Cris;
using CK.DeviceModel.ByTopic.IO.Commands;
using System;
using System.Collections.Generic;

namespace CK.DeviceModel.ByTopic.Commands
{
    public interface ITurnOffMultipleLocationsCommand : ICommand<ISwitchMultipleLocationsCommandResult>
    {
        IList<ITurnOffLocationCommand> Locations { get; }
    }
}
