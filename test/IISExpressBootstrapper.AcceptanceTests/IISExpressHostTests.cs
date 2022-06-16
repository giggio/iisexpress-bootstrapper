using FluentAssertions;
using NUnit.Framework;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace IISExpressBootstrapper.AcceptanceTests
{
    [TestFixture]
    public class IISExpressHostTests
    {
        private IDictionary<string, string> environmentVariables;
        private IISExpressHost host;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            environmentVariables = new Dictionary<string, string> { { "X", "a" }, { "Y", "b" } };
            host = new IISExpressHost(new ConfigFileParameters { TraceLevel = TraceLevel.Info, Systray = false, ConfigFile = SetUpFixture.FileApplicationHostPath, SiteName = "SampleApp" }, environmentVariables).Start();
            using var httpClient = new HttpClient();
            var startTime = DateTime.Now;
            while (true)
            {
                try
                {
                    using var response = await httpClient.GetAsync("http://localhost:8088/", new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
                    break;
                }
                catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
                {
                    if (DateTime.Now - startTime > TimeSpan.FromSeconds(30))
                    {
                        Debug.WriteLine("Could not make the initial request for 30 seconds.");
                        Console.WriteLine("Could not make the initial request for 30 seconds.");
                        throw;
                    }
                    Thread.Sleep(5_000);
                    break;
                }
            }
        }

        [OneTimeTearDown]
        public void TearDown() => host?.Dispose();

        [Test]
        public async Task ShouldRunTheWebApplication()
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync("http://localhost:8088/api/sampleapi/10");
            var content = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("You sent me 10");
        }

        [Test]
        [TestCase("X", "a")]
        [TestCase("Y", "b")]
        public async Task ShouldSetEnvironmentVariablesAsync(string variable, string expected)
        {
            using var client = new HttpClient();
            using var response = await client.PostAsync($"http://localhost:8088/api/sampleapi/{variable}", new StringContent(string.Empty));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = JsonSerializer.Deserialize<string>(await response.Content.ReadAsStringAsync());
            content.Should().Be(expected);
        }

        [Test]
        public void ThrowExceptionWhenNotFoundIISExpressPath()
        {
            const string iisExpressPath = @"Z:\Foo\Bar\iis.exe";
            var action = () => new IISExpressHost(null, iisExpressPath: iisExpressPath);
            action.Should().Throw<IISExpressNotFoundException>();
        }

        [Test]
        public void ThrowExceptionWhenNotFoundWebApplicationPath()
        {
            var action = () => new IISExpressHost("Foo.Bar.Web", 8088);
            action.Should().Throw<DirectoryNotFoundException>()
                .WithMessage("Could not infer the web application folder path.");
        }

        [Test]
        public void StartingWithInvalidConfigurationShouldWriteMessage()
        {
            string s = null;
            new IISExpressHost(new ConfigFileParameters { ConfigFile = "", SiteName = "Does not exist" },
                output: message => { s = message; }).Start();
            s.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void IfProcessIsRunningShouldShowIt() => host.IsRunning.Should().BeTrue();

        [Test]
        public void IfProcessIsRunningShouldShowProcessId() => host.ProcessId.Should().BeGreaterThan(0);

        [Test]
        public void IfProcessIsRunningShouldShowInProcessesList()
        {
            using var process = Process.GetProcessById(host.ProcessId);
            process.Should().NotBeNull();
        }

        [Test]
        public void StartingAndKillingAProcessShouldShowHostNotToBeRunning()
        {
            var brokenHost = new IISExpressHost(new ConfigFileParameters { ConfigFile = "", SiteName = "Does not exist" }).Start();
            using var process = Process.GetProcessById(brokenHost.ProcessId);
            process.Kill();
            brokenHost.IsRunning.Should().BeFalse();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ShouldFindCorrectIISPath(bool preferX64)
        {
            if (!Environment.Is64BitProcess)
                return;
            if (Environment.GetEnvironmentVariable("ProgramFiles") == null)
                throw new Exception("Missing ProgramFiles environment variable.");
            if (Environment.GetEnvironmentVariable("ProgramFiles(x86)") == null)
                throw new Exception("Missing ProgramFiles(x86) environment variable.");
            var host = new IISExpressHost(new ConfigFileParameters { TraceLevel = TraceLevel.Info, Systray = false, ConfigFile = SetUpFixture.FileApplicationHostPath, SiteName = "SampleApp" }, environmentVariables, preferX64: preferX64);
            host.IISExpressPath.Should().Be($@"{Environment.GetEnvironmentVariable(preferX64 ? "ProgramFiles" : "ProgramFiles(x86)")}\IIS Express\IISExpress.exe");
        }
    }
}
