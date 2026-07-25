using System;
using System.Threading.Tasks;

namespace StudentRegistry.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IReviewNoteRepository ReviewNotes { get; }
        Task<int> CompleteAsync();
    }
}
