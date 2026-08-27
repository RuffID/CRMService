using CRMService.Application.Abstractions.Database.Repository;
using CRMService.Infrastructure.DataBase.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Xunit;

namespace CRMService.Architecture.Tests
{
    public class UnitOfWorkArchitectureTests
    {
        [Fact]
        public void ProductionAssemblies_GlobalUnitOfWorkTypes_AreRemoved()
        {
            Type[] types =
            [
                .. typeof(IUnitOfWorkScope).Assembly.GetTypes(),
                .. typeof(MainDbUnitOfWorkScope).Assembly.GetTypes()
            ];

            Assert.DoesNotContain(types, type => type.Name is "IUnitOfWork" or "UnitOfWork" or "IOkdeskUnitOfWork" or "OkdeskUnitOfWork");
        }

        [Fact]
        public void Controllers_Constructors_DoNotDependOnRepositoriesOrUnitOfWork()
        {
            Type[] controllerTypes = typeof(CRMService.Web.Controllers.Authorization.LoginController)
                .Assembly
                .GetTypes()
                .Where(type => !type.IsAbstract && typeof(ControllerBase).IsAssignableFrom(type))
                .ToArray();

            Type[] forbiddenParameters = controllerTypes
                .SelectMany(type => type.GetConstructors())
                .SelectMany(constructor => constructor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .Where(type => typeof(IUnitOfWorkScope).IsAssignableFrom(type)
                    || type.Namespace?.Contains(".Database.Repository", StringComparison.Ordinal) == true)
                .Distinct()
                .ToArray();

            Assert.Empty(forbiddenParameters);
        }

        [Fact]
        public void ApplicationRepositoryContracts_DoNotExposePersistenceFrameworkDetails()
        {
            Type[] repositoryContracts = typeof(IUnitOfWorkScope).Assembly.GetTypes()
                .Where(type => type.IsInterface
                    && type.Namespace?.Contains(".Database.Repository", StringComparison.Ordinal) == true)
                .ToArray();

            Type[] exposedTypes = repositoryContracts
                .SelectMany(type => type.GetMethods())
                .SelectMany(method => method.GetParameters().Select(parameter => parameter.ParameterType)
                    .Append(method.ReturnType))
                .SelectMany(FlattenType)
                .Distinct()
                .ToArray();

            Assert.DoesNotContain(exposedTypes, type =>
                type == typeof(IQueryable)
                || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                || type == typeof(Expression)
                || type.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true
                || type.Assembly.GetName().Name == "EFCoreLibrary");

            Assert.DoesNotContain(repositoryContracts.SelectMany(type => type.GetMethods()).SelectMany(method => method.GetParameters()), parameter =>
                parameter.Name is "asNoTracking" or "include" or "predicate");
        }

        private static IEnumerable<Type> FlattenType(Type type)
        {
            yield return type;

            if (!type.IsGenericType)
                yield break;

            foreach (Type argument in type.GetGenericArguments().SelectMany(FlattenType))
                yield return argument;
        }
    }
}
