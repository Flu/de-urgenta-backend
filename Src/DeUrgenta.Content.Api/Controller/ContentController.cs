using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using DeUrgenta.Common.Controllers;
using DeUrgenta.Common.Swagger;
using DeUrgenta.Content.Api.Swagger.Content;
using DeUrgenta.I18n.Service.Models;
using DeUrgenta.I18n.Service.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace DeUrgenta.Content.Api.Controller
{
    [ApiController]
    [Route("content")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize]
    public class ContentController : BaseAuthController
    {
        private readonly IamI18nProvider _i18NProvider;
        private readonly IAmContentProvider _contentProvider;
        private readonly IAmLanguageProvider _languageProvider;
        public ContentController(IamI18nProvider i18NProvider, IAmContentProvider contentProvider, IAmLanguageProvider languageProvider)
        {
            _i18NProvider = i18NProvider;
            _contentProvider = contentProvider;
            _languageProvider = languageProvider;
        }
        /// <summary>
        /// Get available content for specifc key
        /// </summary>
        [SwaggerResponse(StatusCodes.Status200OK, "Available Content for key", typeof(StringResourceModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A business rule was violated", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Something bad happened", typeof(ProblemDetails))]

        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAvailableContentResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BusinessRuleViolationResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ApplicationErrorResponseExample))]
        [HttpGet]
        public async Task<ActionResult<StringResourceModel>> GetAvailableContent([FromQuery] string key, CancellationToken ct)
        {
            var text = await _i18NProvider.Localize(key);
            return Ok(new StringResourceModel { Key = key, Value = text });
        }


        /// <summary>
        /// Get a list of language specific content keys
        /// </summary>
        [SwaggerResponse(StatusCodes.Status200OK, "Available content keys for Accept-Header language", typeof(IImmutableList<string>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A business rule was violated", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Something bad happened", typeof(ProblemDetails))]

        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAvailableContentKeysResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BusinessRuleViolationResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ApplicationErrorResponseExample))]
        [HttpGet("content_keys")]
        public async Task<ActionResult<IImmutableList<string>>> GetAvailableContentKeys(CancellationToken ct)
        {
            var hasLanguageHeader = HttpContext.Request.Headers
            .TryGetValue("Accept-Language", out var langVal);

            if (!hasLanguageHeader) return BadRequest();

            var languageKeys = await _contentProvider.GetAvailableContentKeys(langVal.ToString());

            return Ok(languageKeys);
        }


        /// <summary>
        /// Get a list of all language models
        /// </summary>
        [SwaggerResponse(StatusCodes.Status200OK, "Available languages", typeof(IImmutableList<LanguageModel>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A business rule was violated", typeof(ProblemDetails))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Something bad happened", typeof(ProblemDetails))]

        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(GetAvailableLanguagesResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BusinessRuleViolationResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(ApplicationErrorResponseExample))]
        [HttpGet("languages")]
        public async Task<ActionResult<IImmutableList<LanguageModel>>> GetAvailableLanguages(CancellationToken ct)
        {
            var languages = await _languageProvider.GetLanguages();
            return Ok(languages);
        }
    }
}