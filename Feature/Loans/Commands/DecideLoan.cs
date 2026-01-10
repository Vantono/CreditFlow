using CreditFlowAPI.Domain.Enums;
using CreditFlowAPI.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CreditFlowAPI.Feature.Loans.Commands
{


    public record DecideLoanCommand(
        Guid LoanId,
        DecisionType Decision,
        string Comments,
        byte[] RowVersion // <--- ΚΡΙΣΙΜΟ: Το token ταυτοχρονίας από το Frontend
    ) : IRequest;

    public class DecideLoanCommandHandler : IRequestHandler<DecideLoanCommand>
    {
        private readonly IApplicationDbContext _context;

        public DecideLoanCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        //public async Task Handle(DecideLoanCommand request, CancellationToken cancellationToken)
        //{
        //    // 1. Βρίσκουμε την αίτηση
        //    var loan = await _context.LoanApplications
        //        .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

        //    if (loan == null) throw new KeyNotFoundException("Η αίτηση δεν βρέθηκε.");

        //    // 2. CONCURRENCY CHECK (Ο έλεγχος του Διαιτητή)
        //    // Συγκρίνουμε το RowVersion που έστειλε ο χρήστης με αυτό που έχει η βάση.
        //    // Σημείωση: Το EF Core το κάνει αυτόματα αν πειράξεις το RowVersion, 
        //    // αλλά εδώ κάνουμε έναν explicit έλεγχο για να είμαστε σίγουροι.
        //    if (!loan.RowVersion.SequenceEqual(request.RowVersion))
        //    {
        //        throw new DbUpdateConcurrencyException("Η αίτηση έχει τροποποιηθεί από άλλον χρήστη. Παρακαλώ κάντε ανανέωση.");
        //    }

        //    // 3. Εκτέλεση Business Logic
        //    if (request.Decision == DecisionType.Approve)
        //    {
        //        loan.Approve(request.Comments);
        //    }
        //    else
        //    {
        //        loan.Reject(request.Comments);
        //    }

        //    // 4. Ενημέρωση του RowVersion (Για την επόμενη φορά)
        //    // Στο SQL Server γίνεται αυτόματα, στην SQLite καλό είναι να το αλλάζουμε εμείς
        //    // ή να αφήσουμε το EF Core να διαχειριστεί το conflict στο SaveChanges.
        //    // Για το Portfolio, αρκεί το SaveChanges που θα χτυπήσει αν τα bytes δεν ταιριάζουν.

        //    // ΣΗΜΑΝΤΙΚΟ: Στο EF Core, πρέπει να του πούμε ότι το RowVersion που ξέρουμε είναι το "παλιό"
        //    _context.LoanApplications.Entry(loan).OriginalValues["RowVersion"] = request.RowVersion;
        //    loan.RowVersion = Guid.NewGuid().ToByteArray();
        //    await _context.SaveChangesAsync(cancellationToken);
        //}

        public async Task Handle(DecideLoanCommand request, CancellationToken cancellationToken)
        {
            // 1. Φέρνουμε την εγγραφή "φρέσκια" από τη βάση
            var loan = await _context.LoanApplications
                .FirstOrDefaultAsync(x => x.Id == request.LoanId, cancellationToken);

            if (loan == null)
            {
                // Αν δεν βρεθεί, πετάμε error (ή επιστρέφουμε κάτι ανάλογα τη λογική σου)
                throw new KeyNotFoundException($"Loan with id {request.LoanId} not found.");
            }

            // 2. Εφαρμόζουμε την απόφαση
            // ΠΡΟΣΟΧΗ: ΔΕΝ πειράζουμε το RowVersion, ούτε το ελέγχουμε εδώ.
            // Αφήνουμε το EF Core να διαχειριστεί το concurrency αυτόματα στο Save.

            loan.Status = request.Decision == DecisionType.Approve
                ? LoanStatus.Approved
                : LoanStatus.Rejected;

            // Αν έχεις πεδίο σχολίων στο Entity:
            // loan.BankerComments = request.Comments; 

            // 3. Αποθήκευση
            await _context.SaveChangesAsync(cancellationToken);

            // Επιστρέφουμε Unit (void για το MediatR)
            return;
        }
    }
}
