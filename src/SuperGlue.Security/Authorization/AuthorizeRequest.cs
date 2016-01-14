using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SuperGlue.Security.Authentication;

namespace SuperGlue.Security.Authorization
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class AuthorizeRequest
    {
        private readonly AppFunc _next;

        private static readonly Cache<Type, Func<object, object, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>>> AuthorizationValidatorExecutors = new Cache<Type, Func<object, object, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>>>();

        public AuthorizeRequest(AppFunc next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var tokens = environment.ResolveAll<IAuthenticationTokenSource>().Select(x => x.GetToken(environment)).ToList();

            var authorizationInformations = new List<IAuthorizationInformation>();

            foreach (var service in environment.ResolveAll<IFindRequiredAuthorizationInformationFromRequest>())
            {
                authorizationInformations.AddRange(await service.FindFor(environment));
            }

            var isAuthorized = true;

            foreach (var authorizationInformation in authorizationInformations)
            {
                var currentAuthorizationInformation = authorizationInformation;

                var authorizationValidators = environment.ResolveAll(typeof(IValidateAuthorizationInformation<>).MakeGenericType(currentAuthorizationInformation.GetType()));

                foreach (var authorizationValidator in authorizationValidators)
                {
                    var currentAuthorizationValidator = authorizationValidator;

                    if (!await AuthorizationValidatorExecutors.Get(currentAuthorizationValidator.GetType(), 
                        x => CompileExecutionFunctionFor(currentAuthorizationValidator.GetType(), currentAuthorizationInformation.GetType()))(currentAuthorizationValidator, currentAuthorizationInformation, tokens, environment))
                    {
                        isAuthorized = false;
                        break;
                    }
                }
            }

            if (isAuthorized)
                await _next(environment);
            else
                environment.FailAuthorization();
        }

        protected virtual Func<object, object, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>> CompileExecutionFunctionFor(Type validatorType, Type authorizationInformationType)
        {
            return (Func<object, object, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>>)GetType()
                .GetMethod("CompileExecutionFunctionForGeneric", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(validatorType, authorizationInformationType)
                .Invoke(this, new object[0]);
        }

        protected virtual Func<object, object, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>> CompileExecutionFunctionForGeneric<TValidator, TAuthorizationInformation>()
            where TAuthorizationInformation : IAuthorizationInformation
            where TValidator : IValidateAuthorizationInformation<TAuthorizationInformation>
        {
            var validatorType = typeof(TValidator);
            var authorizationInformationType = typeof(TAuthorizationInformation);

            var method = validatorType.GetMethod("IsValid", new[] { authorizationInformationType, typeof(IEnumerable<AuthenticationToken>), typeof(IDictionary<string, object>) });

            var validatorParameter = Expression.Parameter(validatorType);
            var authorizationInformationParameter = Expression.Parameter(authorizationInformationType);
            var tokensParameter = Expression.Parameter(typeof(IEnumerable<AuthenticationToken>));
            var environmentParameter = Expression.Parameter(typeof(IDictionary<string, object>));

            var execute = Expression
                .Lambda<Func<TValidator, TAuthorizationInformation, IEnumerable<AuthenticationToken>, IDictionary<string, object>, Task<bool>>>
                    (Expression.Call(validatorParameter, method, authorizationInformationParameter, tokensParameter, environmentParameter), validatorParameter, authorizationInformationParameter, tokensParameter, environmentParameter)
                .Compile();

            return ((validator, authorizationInformation, tokens, environment) => execute((TValidator)validator, (TAuthorizationInformation)authorizationInformation, tokens, environment));
        }
    }
}