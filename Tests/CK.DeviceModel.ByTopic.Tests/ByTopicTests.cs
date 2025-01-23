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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                r.Topics.Add( topic );
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

            var errorUsersMessages = result.UserMessages.Where( x => x.Level == UserMessageLevel.Error ).ToList();
            Assert.That( errorUsersMessages.Count, Is.EqualTo( 1 ) );
            Assert.That( errorUsersMessages[0].Text == $"{deviceHostName} not found in hosts" );
        }

    }
    public static IEnumerable<(string topic, List<string> devicesHostNameWhichTopicNotExist, List<string> deviceHostNameTopicExist, StandardColor color)> SimpleTopicSimpleColorTestsCases()
    {
        var color = StandardColor.Red;
        yield return ("*/*/Test", new List<string>(), new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, color);
        yield return ("*/*/Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) }, new List<string>() { nameof( FakeLEDStripHosts ) }, color);
        yield return ("*/*/Test-FakeSignatureDevice", new List<string>() { nameof( FakeLEDStripHosts ) }, new List<string>() { nameof( FakeSignatureDeviceHosts ) }, color);
        yield return ("*/*/Unknown", new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, new List<string>(), color);

        var off = StandardColor.Off;
        yield return ("*/*/Test", new List<string>(), new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, off);
        yield return ("*/*/Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) }, new List<string>() { nameof( FakeLEDStripHosts ) }, off);
        yield return ("*/*/Test-FakeSignatureDevice", new List<string>() { nameof( FakeLEDStripHosts ) }, new List<string>() { nameof( FakeSignatureDeviceHosts ) }, off);
        yield return ("*/*/Unknown", new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, new List<string>(), off);
    }

    [TestCaseSource( nameof( SimpleTopicSimpleColorTestsCases ) )]
    public async Task can_set_topic_color_Async( (string topic, List<string> devicesHostNameWhichTopicNotExist, List<string> deviceHostNameTopicExist, StandardColor color) tc )
    {
        using( var scope = _auto.Services.CreateScope() )
        {
            var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
            var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

            var turnOnCmd = pocoDirectory.Create<ISetTopicColorCommand>( r =>
            {
                r.Topics.Add( tc.topic );
                r.Color = tc.color;
            } );
            var result = await CrisHelper.SendCrisCommandWithResultAsync<ISetTopicColorCommand, ISetTopicCommandResult>( turnOnCmd, TestHelper.Monitor, cbe );

            ClassicAssert.NotNull( result );

            if( tc.devicesHostNameWhichTopicNotExist.Count == 0 )
            {
                ClassicAssert.IsTrue( result.Success );
            }
            else
            {
                ClassicAssert.IsFalse( result.Success );

                var errorUsersMessages = result.UserMessages.Where( x => x.Level == UserMessageLevel.Error ).ToList();
                Assert.That( errorUsersMessages.Count, Is.EqualTo( tc.devicesHostNameWhichTopicNotExist.Count ) );

                var errorUserMessagesText = errorUsersMessages.Select( x => x.Text );
                foreach( var deviceHostName in tc.devicesHostNameWhichTopicNotExist )
                {
                    ClassicAssert.IsTrue( errorUserMessagesText.Contains( MessageHelper.TopicNotFound( tc.topic, deviceHostName ) ) );
                }
            }

            var userMessageInfo = result.UserMessages.Where( x => x.Level == UserMessageLevel.Info ).ToList();
            Assert.That( userMessageInfo.Count, Is.EqualTo( tc.deviceHostNameTopicExist.Count ) );
            var infoUserMessagesText = userMessageInfo.Select( x => x.Text );
            foreach( var deviceHostName in tc.deviceHostNameTopicExist )
            {
                if(tc.color == StandardColor.Off )
                {
                    ClassicAssert.IsTrue( infoUserMessagesText.Contains( MessageHelper.TopicOff( tc.topic, deviceHostName ) ) );

                }
                else
                {
                    ClassicAssert.IsTrue( infoUserMessagesText.Contains( MessageHelper.TopicOn( tc.topic, deviceHostName, tc.color ) ) );

                }
            }
        }
    }

    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test", true,StandardColor.Red )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test", true, StandardColor.Red )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test-FakeLEDStrip", false, StandardColor.Red )]
    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test-FakeLEDStrip", true, StandardColor.Red )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test-FakeSignatureDevice", true, StandardColor.Red )]
    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test-FakeSignatureDevice", false, StandardColor.Red )]
    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test", true, StandardColor.Off )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test", true, StandardColor.Off )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test-FakeLEDStrip", false, StandardColor.Off )]
    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test-FakeLEDStrip", true, StandardColor.Off )]
    [TestCase( $"{nameof( FakeSignatureDeviceHosts )}/*/Test-FakeSignatureDevice", true, StandardColor.Off )]
    [TestCase( $"{nameof( FakeLEDStripHosts )}/*/Test-FakeSignatureDevice", false, StandardColor.Off )]
    public async Task can_force_set_topic_color_specific_device_Async( string topic, bool isSuccess, StandardColor color )
    {
        using( var scope = _auto.Services.CreateScope() )
        {
            var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
            var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

            var turnOnCmd = pocoDirectory.Create<ISetTopicColorCommand>( r =>
            {
                r.Topics.Add( topic );
                r.Color = color;
            } );
            var result = await CrisHelper.SendCrisCommandWithResultAsync<ISetTopicColorCommand, ISetTopicCommandResult>( turnOnCmd, TestHelper.Monitor, cbe );

            ClassicAssert.NotNull( result );
            Assert.That( result.Success == isSuccess );
            Assert.That( result.UserMessages.Count == 1 );

            var deviceHostName = topic.Split( "/" ).First();

            if( isSuccess )
            {
                if(color is StandardColor.Off )
                {
                    Assert.That( result.UserMessages[0].Text == MessageHelper.TopicOff( topic, deviceHostName ) );
                }
                else
                {
                    Assert.That( result.UserMessages[0].Text == MessageHelper.TopicOn( topic, deviceHostName, color ) );
                }
            }
            else
            {
                Assert.That( result.UserMessages[0].Text == MessageHelper.TopicNotFound( topic, deviceHostName ) );

            }
        }
    }

    public static IEnumerable<(string topic, List<string> devicesHostNameWhichTopicNotExist, List<string> deviceHostNameTopicExist,List<StandardColor> colors)> SimpleTopicMultiColorTestsCases()
    {
        var colors = new List<StandardColor>() { StandardColor.Red, StandardColor.White, StandardColor.Magenta };
        yield return ("*/*/Test", new List<string>(), new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, colors);
        yield return ("*/*/Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) }, new List<string>() { nameof( FakeLEDStripHosts ) }, colors);
        yield return ("*/*/Test-FakeSignatureDevice", new List<string>() { nameof(FakeLEDStripHosts) }, new List<string>() { nameof(FakeSignatureDeviceHosts) }, colors);
        yield return ("*/*/Unknown", new List<string>() { nameof(FakeLEDStripHosts), nameof(FakeSignatureDeviceHosts) }, new List<string>(), colors);

        var colorsMixWithOff = new List<StandardColor>() { StandardColor.Red, StandardColor.Off, StandardColor.Magenta };
        yield return ("*/*/Test", new List<string>(), new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, colorsMixWithOff);
        yield return ("*/*/Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) }, new List<string>() { nameof( FakeLEDStripHosts ) }, colorsMixWithOff);
        yield return ("*/*/Test-FakeSignatureDevice", new List<string>() { nameof( FakeLEDStripHosts ) }, new List<string>() { nameof( FakeSignatureDeviceHosts ) }, colorsMixWithOff);
        yield return ("*/*/Unknown", new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, new List<string>(), colorsMixWithOff);

        var offs = new List<StandardColor>() { StandardColor.Off, StandardColor.Off, StandardColor.Off };
        yield return ("*/*/Test", new List<string>(), new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, offs);
        yield return ("*/*/Test-FakeLEDStrip", new List<string>() { nameof( FakeSignatureDeviceHosts ) }, new List<string>() { nameof( FakeLEDStripHosts ) }, offs);
        yield return ("*/*/Test-FakeSignatureDevice", new List<string>() { nameof( FakeLEDStripHosts ) }, new List<string>() { nameof( FakeSignatureDeviceHosts ) }, offs);
        yield return ("*/*/Unknown", new List<string>() { nameof( FakeLEDStripHosts ), nameof( FakeSignatureDeviceHosts ) }, new List<string>(), offs);
    }

    [TestCaseSource( nameof( SimpleTopicMultiColorTestsCases ) )]
    public async Task can_set_topic_multiple_colors_Async( (string topic, List<string> devicesHostNameWhichTopicNotExist, List<string> deviceHostNameTopicExist, List<StandardColor> colors) tc )
    {
        using( var scope = _auto.Services.CreateScope() )
        {
            var cbe = scope.ServiceProvider.GetRequiredService<CrisBackgroundExecutor>();
            var pocoDirectory = scope.ServiceProvider.GetRequiredService<PocoDirectory>();

            var turnOnCmd = pocoDirectory.Create<ISetTopicMultiColorCommand>( r =>
            {
                r.Topics.Add( tc.topic );
                r.Colors.AddRange( tc.colors );
            } );
            var result = await CrisHelper.SendCrisCommandWithResultAsync<ISetTopicMultiColorCommand, ISetTopicCommandResult>( turnOnCmd, TestHelper.Monitor, cbe );

            ClassicAssert.NotNull( result );

            if( tc.devicesHostNameWhichTopicNotExist.Count == 0 )
            {
                ClassicAssert.IsTrue( result.Success );
            }
            else
            {
                ClassicAssert.IsFalse( result.Success );

                var errorUsersMessages = result.UserMessages.Where( x => x.Level == UserMessageLevel.Error ).ToList();
                Assert.That( errorUsersMessages.Count, Is.EqualTo( tc.devicesHostNameWhichTopicNotExist.Count ) );

                var errorUserMessagesText = errorUsersMessages.Select( x => x.Text );
                foreach( var deviceHostName in tc.devicesHostNameWhichTopicNotExist )
                {
                    ClassicAssert.IsTrue( errorUserMessagesText.Contains( MessageHelper.TopicNotFound( tc.topic, deviceHostName ) ) );
                }
            }

            var userMessageInfo = result.UserMessages.Where( x => x.Level == UserMessageLevel.Info ).ToList();
            Assert.That( userMessageInfo.Count, Is.EqualTo( tc.deviceHostNameTopicExist.Count ) );
            var infoUserMessagesText = userMessageInfo.Select( x => x.Text );
            foreach( var deviceHostName in tc.deviceHostNameTopicExist )
            {
                if( tc.colors.All( x => x == StandardColor.Off ) )
                {
                    ClassicAssert.IsTrue( infoUserMessagesText.Contains( MessageHelper.TopicOff( tc.topic, deviceHostName ) ) );
                }
                else
                {
                    ClassicAssert.IsTrue( infoUserMessagesText.Contains( MessageHelper.TopicOnMultiColor( tc.topic, deviceHostName, tc.colors ) ) );
                }
            }
        }

    }
}

public class Topic
{
    public string Name { get; set; }
    public List<string> TargetDeviceHostName { get; set; } = new List<string>();
}

