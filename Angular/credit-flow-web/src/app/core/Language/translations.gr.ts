export const GR_TRANSLATIONS: Record<string, string> = {
  // Common
  'common.accept': 'Αποδοχή',
  'common.cancel': 'Ακύρωση',

  // Header
  'header.brand': 'CreditFlow',
  'header.role.banker': 'Τραπεζικός',
  'header.role.customer': 'Πελάτης',
  'header.logout': 'Αποσύνδεση',

  // Banker Dashboard
  'bankerDashboard.title': 'Πίνακας Τραπεζικού',
  'bankerDashboard.subtitle': 'Αξιολόγηση και διαχείριση εκκρεμών αιτήσεων δανείου',

  'bankerDashboard.stats.pending': 'Εκκρεμείς Αιτήσεις',
  'bankerDashboard.stats.totalVolume': 'Συνολικός Όγκος',
  'bankerDashboard.stats.lowRisk': 'Χαμηλού Ρίσκου',
  'bankerDashboard.stats.highRisk': 'Υψηλού Ρίσκου',

  'bankerDashboard.table.title': 'Αιτήσεις Δανείου',
  'bankerDashboard.table.searchPlaceholder': 'Αναζήτηση αιτήσεων...',

  'bankerDashboard.table.header.applicant': 'Αιτών',
  'bankerDashboard.table.header.amount': 'Ποσό',
  'bankerDashboard.table.header.monthlyPayment': 'Μηνιαία Δόση',
  'bankerDashboard.table.header.dtiRatio': 'Λόγος DTI',
  'bankerDashboard.table.header.riskLevel': 'Επίπεδο Ρίσκου',
  'bankerDashboard.table.header.submitted': 'Υποβλήθηκε',
  'bankerDashboard.table.header.actions': 'Ενέργειες',

  'bankerDashboard.termMonths': 'μήνες',
  'bankerDashboard.years': 'έτη',
  'bankerDashboard.perMonth': '/μήνα',
  'bankerDashboard.apr': 'APR',

  'bankerDashboard.tooltip.viewDetails': 'Προβολή Λεπτομερειών',
  'bankerDashboard.tooltip.approve': 'Έγκριση',
  'bankerDashboard.tooltip.reject': 'Απόρριψη',

  'bankerDashboard.empty.title': 'Δεν υπάρχουν εκκρεμείς αιτήσεις',
  'bankerDashboard.empty.subtitle': 'Όλες οι αιτήσεις έχουν αξιολογηθεί. Μπράβο!',

  'bankerDashboard.dialog.detailsTitle': 'Αξιολόγηση Αίτησης Δανείου',

  'bankerDashboard.section.applicant': 'Στοιχεία Αιτούντος',
  'bankerDashboard.section.financial': 'Οικονομικά Στοιχεία',
  'bankerDashboard.section.loanDetails': 'Στοιχεία Δανείου',
  'bankerDashboard.section.riskAssessment': 'Αξιολόγηση Ρίσκου',

  'bankerDashboard.label.name': 'Όνομα:',
  'bankerDashboard.label.email': 'Email:',
  'bankerDashboard.label.employer': 'Εργοδότης:',
  'bankerDashboard.label.jobTitle': 'Θέση Εργασίας:',
  'bankerDashboard.label.yearsEmployed': 'Έτη Εργασίας:',
  'bankerDashboard.label.monthlyIncome': 'Μηνιαίο Εισόδημα:',
  'bankerDashboard.label.monthlyExpenses': 'Μηνιαία Έξοδα:',
  'bankerDashboard.label.netCashFlow': 'Καθαρή Ροή Μετρητών:',
  'bankerDashboard.label.requestedAmount': 'Ζητούμενο Ποσό:',
  'bankerDashboard.label.term': 'Διάρκεια:',
  'bankerDashboard.label.interestRate': 'Επιτόκιο:',
  'bankerDashboard.label.monthlyPayment': 'Μηνιαία Δόση:',
  'bankerDashboard.label.totalInterest': 'Συνολικός Τόκος:',
  'bankerDashboard.label.totalCost': 'Συνολικό Κόστος:',
  'bankerDashboard.label.purpose': 'Σκοπός:',

  'bankerDashboard.risk.dtiRatio': 'Λόγος Χρέους/Εισοδήματος',
  'bankerDashboard.risk.riskLevel': 'Επίπεδο Ρίσκου',
  'bankerDashboard.risk.aboveThreshold': 'Πάνω από το ασφαλές όριο (43%)',
  'bankerDashboard.risk.withinRange': 'Εντός ασφαλούς ορίου',

  'bankerDashboard.dialog.close': 'Κλείσιμο',
  'bankerDashboard.dialog.reject': 'Απόρριψη',
  'bankerDashboard.dialog.approve': 'Έγκριση',

  'bankerDashboard.decision.title.approve': 'Έγκριση Αίτησης Δανείου',
  'bankerDashboard.decision.title.reject': 'Απόρριψη Αίτησης Δανείου',
  'bankerDashboard.decision.message.approve': 'Πρόκειται να εγκρίνετε την αίτηση δανείου για',
  'bankerDashboard.decision.message.reject': 'Πρόκειται να απορρίψετε την αίτηση δανείου για',
  'bankerDashboard.decision.warning.reject': 'Η ενέργεια δεν μπορεί να αναιρεθεί. Παρακαλώ δώστε σαφή αιτιολόγηση.',

  'bankerDashboard.decision.label.amount': 'Ποσό:',
  'bankerDashboard.decision.label.monthlyPayment': 'Μηνιαία Δόση:',
  'bankerDashboard.decision.label.riskLevel': 'Επίπεδο Ρίσκου:',

  'bankerDashboard.decision.commentsLabel': 'Σχόλια (Υποχρεωτικό)',
  'bankerDashboard.decision.commentsPlaceholderApprove': 'Δώστε σημειώσεις έγκρισης και τυχόν όρους...',
  'bankerDashboard.decision.commentsPlaceholderReject': 'Εξηγήστε τον λόγο απόρριψης...',
  'bankerDashboard.decision.commentsError': 'Τα σχόλια είναι υποχρεωτικά (10-500 χαρακτήρες)',

  'bankerDashboard.decision.cancel': 'Ακύρωση',
  'bankerDashboard.decision.confirmApprove': 'Επιβεβαίωση Έγκρισης',
  'bankerDashboard.decision.confirmReject': 'Επιβεβαίωση Απόρριψης'
};
