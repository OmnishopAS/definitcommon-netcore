using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Definit.Common.DAL
{
    public static class ModelBuilderExtenions
    {
        public static void AddEntityTypeConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, Func<Type, bool> typeFilter = null)
        {
            var mappingInterfaceOpen = typeof(IEntityTypeConfiguration<>);
            var mappingTypes = assembly.GetMappingTypes(mappingInterfaceOpen);

            var configureMethodOpen = typeof(ModelBuilderExtenions).GetMethod("InvokeConfigure", BindingFlags.NonPublic | BindingFlags.Static);
            foreach(var entConfigType in mappingTypes)
            {
                if(typeFilter!=null && !typeFilter(entConfigType))
                {
                    continue;
                }

                var mappingInterfaceClosed = entConfigType.GetInterfaces().Single(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == mappingInterfaceOpen);
                var entityType = mappingInterfaceClosed.GetGenericArguments()[0];
                var configureMethodClosed = configureMethodOpen.MakeGenericMethod(entityType);
                configureMethodClosed.Invoke(null, new object[] { modelBuilder, entConfigType });
            }
        }

        private static void InvokeConfigure<Tentity>(ModelBuilder modelBuilder, Type configurationClassType)
            where Tentity : class
        {
            var instance = Activator.CreateInstance(configurationClassType) as IEntityTypeConfiguration<Tentity>;
            instance.Configure(modelBuilder.Entity<Tentity>());
        }

        private static IEnumerable<Type> GetMappingTypes(this Assembly assembly, Type mappingInterface)
        {
            return assembly
                .GetTypes()
                .Where(x =>
                    !x.GetTypeInfo().IsAbstract &&
                    x.GetInterfaces().Any(y => y.GetTypeInfo().IsGenericType && y.GetGenericTypeDefinition() == mappingInterface));
        }

    }
}
