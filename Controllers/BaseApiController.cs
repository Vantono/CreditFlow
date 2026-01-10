using CreditFlowAPI.Base.Service;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator? _mediator;
        private ICurrentUserService? _currentUserService;
        private IAuditService? _auditService;

        protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();
        protected ICurrentUserService CurrentUser => _currentUserService ??= HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();
        protected IAuditService AuditService => _auditService ??= HttpContext.RequestServices.GetRequiredService<IAuditService>();
    }
}