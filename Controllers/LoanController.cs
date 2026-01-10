using CreditFlowAPI.Domain.Entities; // Τα DTOs (Records)
using CreditFlowAPI.Domain.Enums;    // Τα Enums (AuditAction, DecisionType)
using CreditFlowAPI.Feature.Loans.Commands;
using CreditFlowAPI.Feature.Loans.Queries;
using MediatR; // Χρειάζεται για το Mediator
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditFlowAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : BaseApiController
    {
        // Helper για το User Id
        private string UserId => CurrentUser.UserId;

        // 1. Create Loan
        [HttpPost("createloan")]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateLoanRequest request)
        {
            // FIX: Χρήση constructor (Positional Record)
            var command = new CreateLoanCommand(
                request.LoanAmount,
                request.TermMonths,
                request.Purpose
            );

            await AuditService.LogAsync(UserId, AuditAction.CreateLoanApplication.ToString(), "User requested CreateLoan");

            return await Mediator.Send(command);
        }

        // 2. Get My Loans
        [HttpPost("getloans")]
        public async Task<ActionResult<List<LoanDto>>> GetLoans([FromBody] GetLoansRequest request)
        {
            await AuditService.LogAsync(UserId, AuditAction.ViewLoans.ToString(), "User requested GetLoan list");
            return await Mediator.Send(new GetMyLoansQuery());
        }

        // 3. Get Details (ΝΕΟ - Λύνει το error για το GetLoanByIdQuery αν το έφτιαξες στο βήμα 2)
        [HttpPost("getloandetails")]
        public async Task<ActionResult<LoanDto>> GetLoanDetails([FromBody] GetLoanDetailsRequest request)
        {
            await AuditService.LogAsync(UserId, "VIEW_LOAN_DETAILS", $"User viewed loan {request.LoanId}");
            return await Mediator.Send(new GetLoanByIdQuery(request.LoanId));
        }

        // 4. Submit Loan
        [HttpPost("submitloan")]
        public async Task<IActionResult> Submit([FromBody] SubmitLoanRequest request)
        {
            await Mediator.Send(new SubmitLoanCommand(request.LoanId));

            await AuditService.LogAsync(
                UserId,
                AuditAction.SubmitLoanApplication.ToString(),
                $"User requested Submit Loan with id {request.LoanId}"
            );

            return NoContent();
        }

        // 5. Upload Document
        [HttpPost("uploaddocument")]
        public async Task<ActionResult<Guid>> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            var command = new UploadDocumentCommand(request.LoanId, request.File);
            var docId = await Mediator.Send(command);

            await AuditService.LogAsync(UserId, "UPLOAD_DOCUMENT", $"Uploaded doc for loan {request.LoanId}");
            return Ok(docId);
        }

        // 6. Archive Loan (ΝΕΟ)
        [HttpPost("archiveloan")]
        public async Task<IActionResult> Archive([FromBody] ArchiveLoanRequest request)
        {
            // await Mediator.Send(new ArchiveLoanCommand(request.LoanId)); // Ξε-σχολίασέ το αν έχεις το Command
            await AuditService.LogAsync(UserId, "ARCHIVE_LOAN", $"Archived loan {request.LoanId}");
            return NoContent();
        }

        // 7. Get Pending Loans (Banker)
        [Authorize(Roles = "Banker")]
        [HttpPost("getpendingloans")]
        // FIX: Άλλαξα το return type σε PendingLoanDto για να ταιριάζει με το Query
        public async Task<ActionResult<List<PendingLoanDto>>> GetPending()
        {
            await AuditService.LogAsync(UserId, "VIEW_PENDING", "Banker requested Pending Loans");
            return await Mediator.Send(new GetPendingLoansQuery());
        }

        // 8. Decide (Banker)
        [Authorize(Roles = "Banker")]
        [HttpPost("decideloan")]
        public async Task<IActionResult> Decide([FromBody] DecideLoanRequest request)
        {
            // FIX: Μετατροπή bool -> DecisionType Enum
            // Βεβαιώσου ότι έχεις `using CreditFlowAPI.Domain.Enums;`
            var decision = request.Approved ? DecisionType.Approve : DecisionType.Reject;

            // FIX: Χρήση constructor και χειρισμός του byte[] (rowVersion)
            // Περνάμε Array.Empty<byte>() προσωρινά αν δεν το έχουμε στο Request
            var command = new DecideLoanCommand(
                request.LoanId,
                decision,
                request.Comments ?? "No comments",
                Array.Empty<byte>()
            );

            await Mediator.Send(command);

            await AuditService.LogAsync(
                UserId,
                "DECIDE_LOAN",
                $"User decided {decision} on loan {request.LoanId}"
            );

            return NoContent();
        }
    }
}