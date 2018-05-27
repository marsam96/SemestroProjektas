using System.Reflection;
using Funq;
using ServiceStack;
using ServiceStack.Api.Swagger;
using ServiceStack.Auth;
using ServiceStack.Caching;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Kursinis
{
    public class AppHost : AppHostBase
    {
        public AppHost(params Assembly[] assembliesWithServices) : base("Kursinis", assembliesWithServices)
        {
        }

        public override void Configure(Container container)
        {
            container.Register<IDbConnectionFactory>(c =>
    new OrmLiteConnectionFactory("server=localhost;user id=root;password=admin;port=3306;database=kursinis;SslMode=none", MySqlDialect.Provider)); //InMemory Sqlite DB

            Plugins.Add(new SwaggerFeature());
            Plugins.Add(new AuthFeature(() => new AuthUserSession(),
            new IAuthProvider[] {
                new BasicAuthProvider(), //Sign-in with HTTP Basic Auth
                new CredentialsAuthProvider(), //HTML Form post of UserName/Password credentials
            }));

            Plugins.Add(new RegistrationFeature());

            container.Register<IAuthRepository>(p => new OrmLiteAuthRepository(p.Resolve<IDbConnectionFactory>()) { UseDistinctRoleTables = true });
            container.Register<IManageRoles>(c => (IManageRoles)c.Resolve<IAuthRepository>());
            var userRepository = container.Resolve<IAuthRepository>();
            userRepository.InitSchema();

        }
    }
}