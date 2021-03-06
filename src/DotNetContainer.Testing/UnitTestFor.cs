using System;
using Autofac;
using Autofac.Extras.Moq;
using DotNetContainer.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;

namespace DotNetContainer.Testing
{
    public abstract class UnitTestFor<T> : IDisposable
    {
        private AutoMock _mocker;
        protected UnitTestFor()
        {
            _mocker = AutoMock.GetLoose();
            ConfigureLogger();
        }

        protected Mock<TMock> Mock<TMock>() where TMock : class
        {
            return _mocker.Mock<TMock>();
        }

        protected void RegisterInstance<TInstance>(TInstance instance) where TInstance : class
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<TInstance>(instance);
            builder.Update(_mocker.Container);
        }

        protected T Component
        {
            get
            {
                return _mocker.Create<T>();
            }
        }

        public void Dispose()
        {
            _mocker.Dispose();
        }

        private void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}")
                .CreateLogger();

            var loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();

            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILoggerFactory>(loggerFactory);
            builder.RegisterSource(new LoggerSource());
            builder.Update(_mocker.Container);
        }
    }
}