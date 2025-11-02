# BUSINESS CONTEXT
Our support team wastes 2 hours daily searching through customer history 
across multiple screens. They need a unified search to find any customer 
interaction quickly.

# WHAT SUCCESS LOOKS LIKE
Support reps can search across all customer data (contacts, calls, emails, 
tickets) from one search box and get results in under 1 second with 
highlighted matches.

# USER STORY
As a support representative, I need to search all customer interactions 
from a single search box so that I can quickly find relevant history 
when a customer calls with a question.

# SPECIFIC REQUIREMENTS
## Must Have:
- Global search box in main navigation bar
- Search across: customer names, phone numbers, email subjects, call notes, ticket descriptions
- Display results grouped by type (Customers, Calls, Emails, Tickets)
- Highlight matching text in results
- Click result to navigate to detail view

## Nice to Have:
- Recent searches dropdown
- Advanced filters (date range, interaction type)
- Keyboard shortcuts (Ctrl+K to focus search)

## Business Rules:
- Minimum 3 characters to trigger search
- Max 50 results per category
- Results ordered by relevance, then date (newest first)

# UI/UX EXPECTATIONS
- Search box should be in top navigation bar, always visible
- Results appear in dropdown panel below search box
- Use existing app color scheme (MaterialDesign primary colors)
- Smooth animations when results appear
- Empty state message when no results found

# DATA REQUIREMENTS
## Search Returns:
- Customer: Name, Phone, Email, Last Contact Date
- Call: Customer Name, Call Date, Duration, Notes (first 100 chars)
- Email: Customer Name, Subject, Date, Snippet (first 100 chars)
- Ticket: Ticket ID, Customer Name, Subject, Status, Created Date

## Data Sources:
- Customers table (SQL Server via EF Core)
- CustomerInteractions table (for calls)
- EmailLog table
- SupportTickets table

# INTEGRATION POINTS
- Uses existing DatabaseContext
- Implement new ISearchService interface
- Add SearchViewModel to bind to search UI
- Wire into existing MainWindowViewModel

# CONSTRAINTS & CONSIDERATIONS
- Performance: Must return results in < 1 second for 100K records
- Security: Users only see data they have permission to access (respect existing RoleService)
- Don't modify existing entity models
- Use async/await for all database calls
- Existing MainWindow layout should accommodate new search box without breaking responsive design

# WORKFLOW
1. User types 3+ characters in search box
2. Debounce 300ms to avoid excessive queries
3. Execute parallel searches across all data sources
4. Aggregate and sort results
5. Display in dropdown grouped by type
6. User clicks result → navigate to appropriate detail view
7. Search dropdown closes automatically

# APPROACH PREFERENCE
Use research-plan-implement workflow. Review existing navigation patterns 
and search implementations (if any) in the codebase first. Show me the 
plan including database query strategy before implementing.

# RELATED CONTEXT
MainWindowViewModel handles navigation. Check CustomerService for 
examples of EF Core query patterns. UI should match style in 
DashboardView.xaml.