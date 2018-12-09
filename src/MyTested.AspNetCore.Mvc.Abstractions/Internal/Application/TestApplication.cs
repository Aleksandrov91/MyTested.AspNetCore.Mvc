﻿namespace MyTested.AspNetCore.Mvc.Internal.Application
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Configuration;
    using Licensing;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Builder;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.DependencyModel;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;
    using Plugins;
    using Services;
    using Utilities.Extensions;
    using Microsoft.DotNet.PlatformAbstractions;

    public static class TestApplication
    {
        private const string TestFrameworkName = "MyTested.AspNetCore.Mvc";
        private const string ReleaseDate = "2016-10-01";

        private static readonly object Sync;

        private static readonly RequestDelegate NullHandler;

        private static readonly ISet<IDefaultRegistrationPlugin> DefaultRegistrationPlugins;
        private static readonly ISet<IServiceRegistrationPlugin> ServiceRegistrationPlugins;
        private static readonly ISet<IRoutingServiceRegistrationPlugin> RoutingServiceRegistrationPlugins;
        private static readonly ISet<IInitializationPlugin> InitializationPlugins;

        private static bool initialiazed;

        private static IConfigurationBuilder configurationBuilder;
        private static TestConfiguration configuration;
        private static GeneralTestConfiguration generalConfiguration;

        private static IHostingEnvironment environment;

        private static Type startupType;

        private static volatile IServiceProvider serviceProvider;
        private static volatile IServiceProvider routingServiceProvider;
        private static volatile IRouter router;

        static TestApplication()
        {
            Sync = new object();

            NullHandler = c => Task.CompletedTask;

            DefaultRegistrationPlugins = new HashSet<IDefaultRegistrationPlugin>();
            ServiceRegistrationPlugins = new HashSet<IServiceRegistrationPlugin>();
            RoutingServiceRegistrationPlugins = new HashSet<IRoutingServiceRegistrationPlugin>();
            InitializationPlugins = new HashSet<IInitializationPlugin>();

            FindTestAssembly();
        }

        public static IServiceProvider Services
        {
            get
            {
                TryLockedInitialization();
                return serviceProvider;
            }
        }

        public static IServiceProvider RoutingServices
        {
            get
            {
                TryLockedInitialization();
                return routingServiceProvider;
            }
        }

        public static IRouter Router
        {
            get
            {
                TryLockedInitialization();
                return router;
            }
        }

        internal static Assembly TestAssembly { get; set; }

        internal static Type StartupType
        {
            get => startupType;

            set
            {
                if (value != null && GeneralConfiguration().NoStartup())
                {
                    throw new InvalidOperationException($"The test configuration ('testconfig.json' file by default) contained 'true' value for the 'General.NoStartup' option but {value.GetName()} class was set through the 'StartsFrom<TStartup>()' method. Either do not set the class or change the option to 'false'.");
                }

                if (startupType != null && GeneralConfiguration().AsynchronousTests())
                {
                    throw new InvalidOperationException("Multiple Startup types per test project while running asynchronous tests is not supported. Either set 'General.AsynchronousTests' in the test configuration ('testconfig.json' file by default) to 'false' or separate your tests into different test projects. The latter is recommended. If you choose the first option, you may need to disable asynchronous testing in your preferred test runner too.");
                }

                if (initialiazed)
                {
                    Reset();
                }

                startupType = value;
            }
        }

        internal static Action<IConfigurationBuilder> AdditionalConfiguration { get; set; }

        internal static Action<IServiceCollection> AdditionalServices { get; set; }

        internal static Action<IApplicationBuilder> AdditionalApplicationConfiguration { get; set; }

        internal static Action<IRouteBuilder> AdditionalRouting { get; set; }

        internal static IHostingEnvironment Environment
        {
            get
            {
                if (environment == null)
                {
                    environment = PrepareEnvironment();
                }

                return environment;
            }
        }

        internal static string ApplicationName
            => GeneralConfiguration().ApplicationName()
                ?? TestAssembly.GetName().Name;

        public static TestConfiguration Configuration()
        {
            if (configuration == null || AdditionalConfiguration != null)
            {
                if (configurationBuilder == null)
                {
                    configurationBuilder = new ConfigurationBuilder()
                        .AddJsonFile("testconfig.json", optional: true);
                }

                AdditionalConfiguration?.Invoke(configurationBuilder);
                AdditionalConfiguration = null;

                configuration = TestConfiguration.With(configurationBuilder.Build());
                generalConfiguration = null;

                PrepareLicensing();
            }

            return configuration;
        }

        public static void TryInitialize()
        {
            lock (Sync)
            {
                var configuration = GeneralConfiguration();

                if (!initialiazed
                    && StartupType == null
                    && !configuration.NoStartup()
                    && configuration.AutomaticStartup())
                {
                    var defaultStartupType = TryFindDefaultStartupType();

                    if (defaultStartupType == null)
                    {
                        throw new InvalidOperationException($"{Environment.EnvironmentName}Startup class could not be found at the root of the test project. Either add it or set 'General.AutomaticStartup' in the test configuration ('testconfig.json' file by default) to 'false'.");
                    }
                    else if (GeneralConfiguration().NoStartup())
                    {
                        throw new InvalidOperationException($"The test configuration ('testconfig.json' file by default) contained 'true' value for the 'General.NoStartup' option but {Environment.EnvironmentName}Startup class was located at the root of the project. Either remove the class or change the option to 'false'.");
                    }

                    startupType = defaultStartupType;
                    Initialize();
                }
            }
        }

        internal static GeneralTestConfiguration GeneralConfiguration()
        {
            if (generalConfiguration == null)
            {
                generalConfiguration = Configuration().General();
            }

            return generalConfiguration;
        }

        internal static DependencyContext LoadDependencyContext()
            => DependencyContext.Load(TestAssembly) ?? DependencyContext.Default;

        internal static void LoadPlugins(DependencyContext dependencyContext)
        {
            var testFrameworkAssemblies = dependencyContext
                .GetRuntimeAssemblyNames(RuntimeEnvironment.GetRuntimeIdentifier())
                .Where(l => l.Name.StartsWith(TestFrameworkName))
                .ToArray();

            if (testFrameworkAssemblies.Length == 7 && testFrameworkAssemblies.Any(t => t.Name == $"{TestFrameworkName}.Lite"))
            {
                TestCounter.SkipValidation = true;
            }

            var plugins = testFrameworkAssemblies
                .Select(l => Assembly.Load(new AssemblyName(l.Name)).GetType($"{TestFrameworkName}.Plugins.{l.Name.Replace(TestFrameworkName, string.Empty).Trim('.')}TestPlugin"))
                .Where(p => p != null)
                .ToArray();

            if (!plugins.Any())
            {
                throw new InvalidOperationException("Test plugins could not be loaded. Depending on your project's configuration you may need to set the 'preserveCompilationContext' property under 'buildOptions' to 'true' in the test assembly's 'project.json' file and/or may need to call '.StartsFrom<TStartup>().WithTestAssembly(this)'.");
            }

            plugins.ForEach(t =>
            {
                var plugin = Activator.CreateInstance(t);

                if (plugin is IDefaultRegistrationPlugin defaultRegistrationPlugin)
                {
                    DefaultRegistrationPlugins.Add(defaultRegistrationPlugin);
                }

                if (plugin is IServiceRegistrationPlugin servicePlugin)
                {
                    ServiceRegistrationPlugins.Add(servicePlugin);
                }

                if (plugin is IRoutingServiceRegistrationPlugin routingServicePlugin)
                {
                    RoutingServiceRegistrationPlugins.Add(routingServicePlugin);
                }

                if (plugin is IInitializationPlugin initializationPlugin)
                {
                    InitializationPlugins.Add(initializationPlugin);
                }

                if (plugin is IHttpFeatureRegistrationPlugin httpFeatureRegistrationPlugin)
                {
                    TestHelper.HttpFeatureRegistrationPlugins.Add(httpFeatureRegistrationPlugin);
                }

                if (plugin is IShouldPassForPlugin shouldPassForPlugin)
                {
                    TestHelper.ShouldPassForPlugins.Add(shouldPassForPlugin);
                }
            });
        }

        internal static Type TryFindDefaultStartupType()
        {
            EnsureTestAssembly();

            var defaultStartupType = GeneralConfiguration().StartupType() ?? $"{Environment.EnvironmentName}Startup";

            // check root of the test project
            var startup =
                TestAssembly.GetType(defaultStartupType) ??
                TestAssembly.GetType($"{TestAssembly.GetName().Name}.{defaultStartupType}");

            return startup;
        }

        private static void Initialize()
        {
            EnsureTestAssembly();

            if (StartupType == null && !GeneralConfiguration().NoStartup())
            {
                throw new InvalidOperationException($"The test configuration ('testconfig.json' file by default) contained 'false' value for the 'General.NoStartup' option but a Startup class was not provided. Either add {Environment.EnvironmentName}Startup class to the root of the test project or set it by calling 'StartsFrom<TStartup>()'. Additionally, if you do not want to use a global test application for all test cases in this project, you may change the test configuration option to 'true'.");
            }

            PrepareLicensing();

            var dependencyContext = LoadDependencyContext();
            LoadPlugins(dependencyContext);

            var serviceCollection = GetInitialServiceCollection();
            var startupMethods = PrepareStartup(serviceCollection);

            PrepareServices(serviceCollection, startupMethods);
            PrepareApplicationAndRouting(startupMethods);

            initialiazed = true;
        }

        private static void PrepareLicensing()
        {
            if (TestAssembly != null)
            {
                TestCounter.SetLicenseData(
                    Configuration().Licenses(),
                    DateTime.ParseExact(ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    TestAssembly.GetName().Name);
            }
        }

        private static IHostingEnvironment PrepareEnvironment()
            => new HostingEnvironment
            {
                ApplicationName = ApplicationName,
                EnvironmentName = GeneralConfiguration().EnvironmentName()
            };

        private static IServiceCollection GetInitialServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var diagnosticListener = new DiagnosticListener(TestFrameworkName);
            
            // default server services
            serviceCollection.AddSingleton(Environment);
            serviceCollection.AddSingleton<IApplicationLifetime, ApplicationLifetime>();
            
            serviceCollection.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>();
            serviceCollection.AddTransient<IHttpContextFactory, HttpContextFactory>();
            serviceCollection.AddScoped<IMiddlewareFactory, MiddlewareFactory>();
            serviceCollection.AddOptions();

            serviceCollection.AddSingleton<ILoggerFactory>(LoggerFactoryMock.Create());
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(diagnosticListener);
            serviceCollection.AddSingleton<DiagnosticSource>(diagnosticListener);

            serviceCollection.AddTransient<IStartupFilter, AutoRequestServicesStartupFilter>();
            serviceCollection.AddTransient<IServiceProviderFactory<IServiceCollection>, DefaultServiceProviderFactory>();

            serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            return serviceCollection;
        }

        private static StartupMethods PrepareStartup(IServiceCollection serviceCollection)
        {
            StartupMethods startupMethods = null;
            if (StartupType != null)
            {
                startupMethods = StartupLoader.LoadMethods(
                    serviceCollection.BuildServiceProvider(),
                    StartupType,
                    Environment.EnvironmentName);

                if (typeof(IStartup).GetTypeInfo().IsAssignableFrom(StartupType.GetTypeInfo()))
                {
                    serviceCollection.AddSingleton(typeof(IStartup), StartupType);
                }
                else
                {
                    serviceCollection.AddSingleton(typeof(IStartup), sp => new ConventionBasedStartup(startupMethods));
                }
            }

            return startupMethods;
        }

        private static void PrepareServices(IServiceCollection serviceCollection, StartupMethods startupMethods)
        {
            if (startupMethods?.ConfigureServicesDelegate != null)
            {
                startupMethods.ConfigureServicesDelegate(serviceCollection);
            }
            else
            {
                var defaultRegistrationPlugin = DefaultRegistrationPlugins
                    .OrderByDescending(p => p.Priority)
                    .FirstOrDefault();

                if (defaultRegistrationPlugin != null)
                {
                    defaultRegistrationPlugin.DefaultServiceRegistrationDelegate(serviceCollection);
                }
                else
                {
                    serviceCollection.AddMvcCore();
                }
            }

            AdditionalServices?.Invoke(serviceCollection);

            TryReplaceKnownServices(serviceCollection);
            PrepareRoutingServices(serviceCollection);

            serviceProvider = serviceCollection.BuildServiceProvider();

            InitializationPlugins.ForEach(plugin => plugin.InitializationDelegate(serviceProvider));
        }

        private static void TryReplaceKnownServices(IServiceCollection serviceCollection)
        {
            var applicablePlugins = new HashSet<IServiceRegistrationPlugin>();

            serviceCollection.ForEach(service =>
            {
                TestServiceProvider.SaveServiceLifetime(service.ServiceType, service.Lifetime);

                foreach (var serviceRegistrationPlugin in ServiceRegistrationPlugins)
                {
                    if (serviceRegistrationPlugin.ServiceSelectorPredicate(service))
                    {
                        applicablePlugins.Add(serviceRegistrationPlugin);
                    }
                }
            });

            foreach (var applicablePlugin in applicablePlugins)
            {
                applicablePlugin.ServiceRegistrationDelegate(serviceCollection);
            }
        }

        private static void PrepareRoutingServices(IServiceCollection serviceCollection)
        {
            var routingServiceCollection = new ServiceCollection().Add(serviceCollection);

            foreach (var routingServiceRegistrationPlugin in RoutingServiceRegistrationPlugins)
            {
                routingServiceRegistrationPlugin.RoutingServiceRegistrationDelegate(routingServiceCollection);
            }

            routingServiceProvider = routingServiceCollection.BuildServiceProvider();
        }

        private static void PrepareApplicationAndRouting(StartupMethods startupMethods)
        {
            var applicationBuilder = new ApplicationBuilderMock(serviceProvider);

            startupMethods?.ConfigureDelegate?.Invoke(applicationBuilder);

            AdditionalApplicationConfiguration?.Invoke(applicationBuilder);

            var routeBuilder = new RouteBuilder(applicationBuilder)
            {
                DefaultHandler = new RouteHandler(NullHandler)
            };

            for (int i = 0; i < applicationBuilder.Routes.Count; i++)
            {
                var route = applicationBuilder.Routes[i];
                routeBuilder.Routes.Add(route);
            }

            AdditionalRouting?.Invoke(routeBuilder);

            if (StartupType == null || routeBuilder.Routes.Count == 0)
            {
                routeBuilder.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routeBuilder.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(serviceProvider));
            }

            router = routeBuilder.Build();
        }

        private static void TryLockedInitialization()
        {
            if (!initialiazed)
            {
                lock (Sync)
                {
                    if (!initialiazed)
                    {
                        Initialize();
                    }
                }
            }
        }

        private static void Reset()
        {
            initialiazed = false;
            configurationBuilder = null;
            configuration = null;
            environment = null;
            startupType = null;
            serviceProvider = null;
            routingServiceProvider = null;
            router = null;
            AdditionalServices = null;
            AdditionalApplicationConfiguration = null;
            AdditionalRouting = null;
            TestAssembly = null;
            TestServiceProvider.Current = null;
            TestServiceProvider.ClearServiceLifetimes();
            DefaultRegistrationPlugins.Clear();
            ServiceRegistrationPlugins.Clear();
            RoutingServiceRegistrationPlugins.Clear();
            InitializationPlugins.Clear();
            LicenseValidator.ClearLicenseDetails();
        }

        private static void FindTestAssembly()
        {
            var testAssemblyName = GeneralConfiguration().TestAssemblyName();
            if (testAssemblyName != null)
            {
                TestAssembly = Assembly.Load(new AssemblyName(testAssemblyName));
            }
            else
            {
#if NET451
                var executingAssembly = Assembly.GetExecutingAssembly();

                var stackTrace = new StackTrace(false);

                foreach (var frame in stackTrace.GetFrames())
                {
                    var method = frame.GetMethod();
                    var methodAssembly = method?.DeclaringType?.Assembly;

                    if (methodAssembly != null
                        && methodAssembly != executingAssembly
                        && !methodAssembly.FullName.StartsWith(TestFrameworkName))
                    {
                        TestAssembly = methodAssembly;
                        break;
                    }
                }
#endif
#if NETSTANDARD1_6
                var assemblyName = DependencyContext
                    .Default
                    .GetDefaultAssemblyNames()
                    .First();

                TestAssembly = Assembly.Load(assemblyName);
#endif
            }
        }

        private static void EnsureTestAssembly()
        {
            if (TestAssembly == null)
            {
                FindTestAssembly();
            }

            if (TestAssembly == null)
            {
                throw new InvalidOperationException("Test assembly could not be loaded. You can specify it explicitly in the test configuration ('testconfig.json' file by default) by providing a value for the 'General.TestAssemblyName' option or set it by calling '.StartsFrom<TStartup>().WithTestAssembly(this)'.");
            }
        }
    }
}
