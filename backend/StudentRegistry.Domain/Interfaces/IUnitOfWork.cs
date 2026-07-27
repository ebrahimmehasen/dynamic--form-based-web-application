using System;
using System.Threading.Tasks;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IReviewNoteRepository ReviewNotes { get; }
        IUserRepository Users { get; }
        IFieldEditRepository FieldEdits { get; }
        IFieldCommentRepository FieldComments { get; }
        IDeleteRequestRepository DeleteRequests { get; }
        IDashboardRepository Dashboard { get; }
        Task<int> CompleteAsync();
    }
}
