using StudentRegistry.Data.DbContext;
using StudentRegistry.Domain.Interfaces;
using System.Threading.Tasks;

namespace StudentRegistry.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StudentRegistryDbContext _context;
        private IStudentRepository? _students;
        private IReviewNoteRepository? _reviewNotes;
        private IUserRepository? _users;
        private IFieldEditRepository? _fieldEdits;
        private IFieldCommentRepository? _fieldComments;
        private IDeleteRequestRepository? _deleteRequests;
        private IPendingReviewRepository? _pendingReviews;
        private IDashboardRepository? _dashboard;

        public UnitOfWork(StudentRegistryDbContext context)
        {
            _context = context;
        }

        public IStudentRepository Students => _students ??= new StudentRepository(_context);
        public IReviewNoteRepository ReviewNotes => _reviewNotes ??= new ReviewNoteRepository(_context);
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IFieldEditRepository FieldEdits => _fieldEdits ??= new FieldEditRepository(_context);
        public IFieldCommentRepository FieldComments => _fieldComments ??= new FieldCommentRepository(_context);
        public IDeleteRequestRepository DeleteRequests => _deleteRequests ??= new DeleteRequestRepository(_context);
        public IPendingReviewRepository PendingReviews => _pendingReviews ??= new PendingReviewRepository(_context);
        public IDashboardRepository Dashboard => _dashboard ??= new DashboardRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
