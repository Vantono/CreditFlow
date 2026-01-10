using CreditFlowAPI.Feature.Loans.Commands;
using CreditFlowAPI.Feature.Loans.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST api/loans
        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateLoanCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, id); // 201 Created
        }

        // GET api/loans
        [HttpGet]
        public async Task<ActionResult<List<LoanDto>>> Get()
        {
            return await _mediator.Send(new GetMyLoansQuery());
        }

        // PUT api/loans/{id}/submit
        [HttpPut("{id}/submit")]
        public async Task<IActionResult> Submit(Guid id)
        {
            // Προσοχή: Το ID στο URL πρέπει να μπει στο Command
            await _mediator.Send(new SubmitLoanCommand(id));
            return NoContent(); // 204 Success
        }

        // POST api/loans/{id}/documents
        [HttpPost("{id}/documents")]
        public async Task<ActionResult<Guid>> UploadDocument(Guid id, IFormFile file)
        {
            var command = new UploadDocumentCommand(id, file);
            var docId = await _mediator.Send(command);
            return Ok(docId);
        }

        // GET api/loans/pending (Για τον Banker)
        [HttpGet("pending")]
        public async Task<ActionResult<List<PendingLoanDto>>> GetPending()
        {
            // Εδώ αργότερα θα βάλουμε [Authorize(Roles = "Banker")]
            return await _mediator.Send(new GetPendingLoansQuery());
        }

        // POST api/loans/{id}/decision
        [HttpPost("{id}/decision")]
        public async Task<IActionResult> Decide(Guid id, DecideLoanCommand command)
        {
            if (id != command.LoanId) return BadRequest();

            await _mediator.Send(command);
            return NoContent();
        }
    }
}
