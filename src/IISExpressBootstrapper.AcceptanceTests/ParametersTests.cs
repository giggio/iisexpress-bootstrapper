﻿using FluentAssertions;
using NUnit.Framework;

namespace IISExpressBootstrapper.AcceptanceTests
{
    [TestFixture]
    public class ParametersTests
    {
        [Test]
        public void DefaultConfigFileParametersShouldBeEmpty()
        {
            var parameters = new ConfigFileParameters();

            parameters.ToString().Should().Be(string.Empty);
        }

        [Test]
        public void DefaultPathParametersShouldBeEmpty()
        {
            var parameters = new PathParameters();

            parameters.ToString().Should().Be(string.Empty);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetParameterSystray(bool value)
        {
            var configFileParameters = new ConfigFileParameters { Systray = value };
            var pathParameters = new PathParameters { Systray = value };

            var expected = " /systray:" + value.ToString().ToLower();

            configFileParameters.ToString().Should().Be(expected);
            pathParameters.ToString().Should().Be(expected);
        }

        [Test]
        [TestCase(TraceLevel.Error)]
        [TestCase(TraceLevel.Info)]
        [TestCase(TraceLevel.Warning)]
        public void SetParameterTraceLevel(TraceLevel traceLevel)
        {
            var configFileParameters = new ConfigFileParameters { TraceLevel = traceLevel };
            var pathParameters = new PathParameters { TraceLevel = traceLevel };

            var expected = " /trace:" + traceLevel.ToString().ToLower();

            configFileParameters.ToString().Should().Be(expected);
            pathParameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterConfigFile()
        {
            const string value = @"C:\MyPath\applicationhosts.config";
            var parameters = new ConfigFileParameters { ConfigFile = value };

            var expected = string.Format(@" /config:""{0}""", value);

            parameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterSiteId()
        {
            const string value = @"Foo";
            var parameters = new ConfigFileParameters { SiteId = value };

            var expected = string.Format(@" /siteid:""{0}""", value);

            parameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterSiteName()
        {
            const string value = @"My Site Name";
            var parameters = new ConfigFileParameters { SiteName = value };

            var expected = string.Format(@" /site:""{0}""", value);

            parameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterClrVersion()
        {
            const string value = @"v4.0";
            var parameters = new PathParameters { ClrVersion = value };

            var expected = string.Format(@" /clr:""{0}""", value);

            parameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterPath()
        {
            const string value = @"C:\My Path\MySiteFolder";
            var parameters = new PathParameters { Path = value };

            var expected = string.Format(@" /path:""{0}""", value);

            parameters.ToString().Should().Be(expected);
        }

        [Test]
        public void SetParameterPort()
        {
            const int value = 8088;
            var parameters = new PathParameters { Port = value };

            var expected = string.Format(@" /port:{0}", value);

            parameters.ToString().Should().Be(expected);
        }
    }
}
