using CK.Core;
using CK.Cris;
using CK.Cris.DeviceModel;
using CK.DeviceModel.ByTopic.Tests.Helpers;
using CK.DeviceModel.ByTopic.Tests.Hosts;
using CK.IO.DeviceModel.ByTopic;
using CK.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DeviceModel.ByTopic.Tests;

public class ByTopicTests
{
    [AllowNull]
    AutomaticServices _auto;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        var configuration = TestHelper.CreateDefaultEngineConfiguration();
        configuration.FirstBinPath.Types.Add( typeof( CrisBackgroundExecutorService ),
                                              typeof( CrisBackgroundExecutor ),
                                              typeof( CrisExecutionContext ),
                                              typeof( ScopedUserMessageCollector ),
                                              typeof( FakeLEDStripHosts ),
                                              typeof( FakeSignatureDeviceHosts ),
                                              typeof( ISetTopicColorCommand ),
                                              typeof( ISetTopicMultiColorCommand ),
                                              typeof( ISetTopicCommandResult ),
                                              typeof( ByTopicCommandHandler ),
                                              typeof( Validators )
                                              );
        var engineResult = await configuration.RunSuccessfullyAsync();
        _auto = engineResult.CreateAutomaticServices();
    }

    [OneTimeTearDown]
    public async Task OneTimeDearDownAsync()
    {
        await _auto.DisposeAsync();
    }

    [Test]
    public async Task validators_should_remove_first_character_when_topic_begining_with_slash_Async()
    {
        using( var scope = _auto.Services.CreateScope() )
        {
            var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
            var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

            var topic = "/*/*/Test";
            var turnOffCmd = pocoDirectory.Create<ISetTopicColorCommand>( r =>
            {
                r.Topics.Add(topic);
                r.Color = StandardColor.Off;
            } );
            var result = await CrisHelper.SendCrisCommandWithResultAsync<ISetTopicColorCommand, ISetTopicCommandResult>( turnOffCmd, TestHelper.Monitor, cbe );

            ClassicAssert.NotNull( result );
            ClassicAssert.IsTrue( result.Success );
        }

    }

    [Test]
    public async Task validators_should_collect_error_when_device_full_name_not_matching_Async()
    {
        using( var scope = _auto.Services.CreateScope() )
        {
            var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
            var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

            var deviceHostName = "?";
            var topic = $"{deviceHostName}/*/Test";
            var turnOffCmd = pocoDirectory.Create<ISetTopicColorCommand>( r =>
            {
                r.Topics.Add( topic );
                r.Color = StandardColor.Off;
            } );

            var result = await CrisHelper.SendCrisCommandWithResultAsync<ISetTopicColorCommand, ISetTopicCommandResult>( turnOffCmd, TestHelper.Monitor, cbe );

            ClassicAssert.NotNull( result );
            ClassicAssert.IsFalse( result.Success );
            ClassicAssert.That(result.UserMessages.Count, Is.EqualTo( 1 ) );
            ClassicAssert.That( result.UserMessages[0].Text == $"{deviceHostName} not found in hosts" );
        }

    }

    //[TestCaseSource( nameof( SwitchTestsCases ) )]
    //public async Task can_turn_on_topic_Async( (string topic, List<string> deviceThatShouldBeFalse) tc )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var turnOnCmd = pocoDirectory.Create<ITurnOnTopicCommand>( r =>
    //        {
    //            r.Topic = tc.topic;
    //            r.Colors.Add( StandardColor.Red );
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOnTopicCommand, ISwitchTopicCommandResult>( turnOnCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.ResultByDeviceName );

    //        foreach( var keyValuePair in result.ResultByDeviceName )
    //        {
    //            if( tc.deviceThatShouldBeFalse.Contains( keyValuePair.Key ) )
    //            {
    //                ClassicAssert.False( keyValuePair.Value );
    //            }
    //            else
    //            {
    //                ClassicAssert.True( keyValuePair.Value );
    //            }
    //        }
    //    }

    //}

    //[TestCaseSource( nameof( SwitchTestsCases ) )]
    //public async Task can_turn_off_topic_Async( (string topic, List<string> deviceThatShouldBeFalse) tc )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var turnOffCmd = pocoDirectory.Create<ITurnOffTopicCommand>( r =>
    //        {
    //            r.Topic = tc.topic;
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOffTopicCommand, ISwitchTopicCommandResult>( turnOffCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.ResultByDeviceName );

    //        foreach( var keyValuePair in result.ResultByDeviceName )
    //        {
    //            if( tc.deviceThatShouldBeFalse.Contains( keyValuePair.Key ) )
    //            {
    //                ClassicAssert.False( keyValuePair.Value );
    //            }
    //            else
    //            {
    //                ClassicAssert.True( keyValuePair.Value );
    //            }
    //        }
    //    }

    //}

    //public static IEnumerable<(string topic, List<string> deviceThatShouldBeFalse)> SwitchTestsCases()
    //{
    //    yield return ("Test", new List<string>());
    //    yield return ("Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) });
    //    yield return ("Test-FakeSignatureDevice", new List<string>() { nameof( FakeLEDStripHosts ) });
    //    yield return ("Unknown", new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) });
    //}

    //[TestCaseSource( nameof( MultipleSwitchTestsCases ) )]
    //public async Task can_turn_on_multiple_topic_Async( Dictionary<string, List<Switch>> tc )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var list = new List<ITurnOnTopicCommand>();
    //        foreach( var item in tc )
    //        {
    //            list.Add( pocoDirectory.Create<ITurnOnTopicCommand>( r =>
    //            {
    //                r.Topic = item.Key;
    //                r.Colors.Add( StandardColor.Red );
    //            } ) );
    //        }

    //        var turnOnMultipleCmd = pocoDirectory.Create<ITurnOnMultipleTopicsCommand>( r =>
    //        {
    //            r.Topics.AddRange( list );
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOnMultipleTopicsCommand, ISwitchMultipleTopicsCommandResult>( turnOnMultipleCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.Results );

    //        foreach( var switchTopicCommandResult in result.Results )
    //        {
    //            var allDevicesOnTopics = tc[switchTopicCommandResult.Topic];
    //            ClassicAssert.NotNull( allDevicesOnTopics );
    //            ClassicAssert.IsNotEmpty( allDevicesOnTopics );

    //            foreach( var keyValuePair in switchTopicCommandResult.ResultByDeviceName )
    //            {
    //                var device = allDevicesOnTopics.First( x => x.DeviceName == keyValuePair.Key );

    //                Assert.That( device.ShouldSuccess, Is.EqualTo( keyValuePair.Value ) );
    //            }
    //        }
    //    }

    //}

    //[TestCaseSource( nameof( MultipleSwitchTestsCases ) )]
    //public async Task can_turn_off_multiple_topic_Async( Dictionary<string, List<Switch>> tc )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var list = new List<ITurnOffTopicCommand>();
    //        foreach( var item in tc )
    //        {
    //            list.Add( pocoDirectory.Create<ITurnOffTopicCommand>( r =>
    //            {
    //                r.Topic = item.Key;
    //            } ) );
    //        }

    //        var turnOffMultipleCmd = pocoDirectory.Create<ITurnOffMultipleTopicsCommand>( r =>
    //        {
    //            r.Topics.AddRange( list );
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOffMultipleTopicsCommand, ISwitchMultipleTopicsCommandResult>( turnOffMultipleCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.Results );

    //        foreach( var switchTopicCommandResult in result.Results )
    //        {
    //            var allDevicesOnTopics = tc[switchTopicCommandResult.Topic];
    //            ClassicAssert.NotNull( allDevicesOnTopics );
    //            ClassicAssert.IsNotEmpty( allDevicesOnTopics );

    //            foreach( var keyValuePair in switchTopicCommandResult.ResultByDeviceName )
    //            {
    //                var device = allDevicesOnTopics.First( x => x.DeviceName == keyValuePair.Key );

    //                Assert.That( device.ShouldSuccess, Is.EqualTo( keyValuePair.Value ) );
    //            }
    //        }
    //    }

    //}

    //public static IEnumerable<Dictionary<string, List<Switch>>> MultipleSwitchTestsCases()
    //{
    //    yield return new Dictionary<string, List<Switch>>()
    //    {
    //        {
    //           "Test-FakeLEDStrip",
    //           new List<Switch>(){
    //               new Switch
    //               {
    //                   DeviceName = nameof(FakeLEDStripHosts)
    //               },
    //               new Switch
    //               {
    //                   DeviceName = nameof(FakeSignatureDeviceHosts),
    //                   ShouldSuccess = false
    //               }
    //           }
    //        },
    //        {
    //           "Test-FakeSignatureDevice",
    //           new List<Switch>(){
    //               new Switch
    //               {
    //                   DeviceName = nameof(FakeSignatureDeviceHosts)
    //               },
    //               new Switch
    //               {
    //                   DeviceName = nameof(FakeLEDStripHosts),
    //                   ShouldSuccess = false
    //               }
    //           }
    //        }
    //    };
    //}

    //[TestCase( "Test", nameof( FakeLEDStripHosts ), true )]
    //[TestCase( "Test", nameof( FakeSignatureDeviceHosts ), true )]
    //[TestCase( "Test-FakeLEDStrip", nameof( FakeSignatureDeviceHosts ), false )]
    //[TestCase( "Test-FakeLEDStrip", nameof( FakeLEDStripHosts ), true )]
    //[TestCase( "Test-FakeSignatureDevice", nameof( FakeSignatureDeviceHosts ), true )]
    //[TestCase( "Test-FakeSignatureDevice", nameof( FakeLEDStripHosts ), false )]
    //public async Task can_force_turn_on_topic_specific_device_Async( string topic, string deviceFullName, bool isSuccess )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var turnOnCmd = pocoDirectory.Create<ITurnOnTopicCommand>( r =>
    //        {
    //            r.Topic = topic;
    //            r.Colors.Add( StandardColor.Red );
    //            r.DeviceFullName = deviceFullName;
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOnTopicCommand, ISwitchTopicCommandResult>( turnOnCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.ResultByDeviceName );
    //        Assert.That( result.ResultByDeviceName.Count, Is.EqualTo( 1 ) );

    //        Assert.That( result.ResultByDeviceName[deviceFullName], Is.EqualTo( isSuccess ) );
    //    }
    //}

    //[TestCase( "Test", nameof( FakeLEDStripHosts ), true )]
    //[TestCase( "Test", nameof( FakeSignatureDeviceHosts ), true )]
    //[TestCase( "Test-FakeLEDStrip", nameof( FakeSignatureDeviceHosts ), false )]
    //[TestCase( "Test-FakeLEDStrip", nameof( FakeLEDStripHosts ), true )]
    //[TestCase( "Test-FakeSignatureDevice", nameof( FakeSignatureDeviceHosts ), true )]
    //[TestCase( "Test-FakeSignatureDevice", nameof( FakeLEDStripHosts ), false )]
    //public async Task can_turn_off_topic_specific_device_Async( string topic, string deviceFullName, bool isSuccess )
    //{
    //    using( var scope = _auto.Services.CreateScope() )
    //    {
    //        var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
    //        var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

    //        var turnOffCmd = pocoDirectory.Create<ITurnOffTopicCommand>( r =>
    //        {
    //            r.Topic = topic;
    //            r.DeviceFullName = deviceFullName;
    //        } );
    //        var result = await CrisHelper.SendCrisCommandWithResultAsync<ITurnOffTopicCommand, ISwitchTopicCommandResult>( turnOffCmd, TestHelper.Monitor, cbe );

    //        ClassicAssert.NotNull( result );
    //        ClassicAssert.IsNotEmpty( result.ResultByDeviceName );
    //        Assert.That( result.ResultByDeviceName.Count, Is.EqualTo( 1 ) );

    //        Assert.That( result.ResultByDeviceName[deviceFullName], Is.EqualTo( isSuccess ) );
    //    }
    //}

}


public class Switch
{
    public string? DeviceName { get; set; }
    public bool ShouldSuccess { get; set; } = true;
}


