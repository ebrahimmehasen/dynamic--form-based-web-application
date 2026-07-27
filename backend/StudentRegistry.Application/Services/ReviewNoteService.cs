using AutoMapper;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Application.Interfaces;
using StudentRegistry.Domain.Entities;
using StudentRegistry.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRegistry.Application.Services
{
    public class ReviewNoteService : IReviewNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewNoteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReviewNoteResponseDto>> GetNotesForStudentAsync(int studentId)
        {
            var notes = await _unitOfWork.ReviewNotes.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<ReviewNoteResponseDto>>(notes);
        }

        public async Task<ReviewNoteResponseDto?> AddNoteAsync(ReviewNoteCreateDto createDto, string authorUsername)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(createDto.StudentId);
            if (student == null)
            {
                return null;
            }

            var note = new ReviewNote
            {
                StudentId = createDto.StudentId,
                FieldName = createDto.FieldName,
                FieldValueSnapshot = createDto.FieldValueSnapshot,
                ReviewerNote = createDto.ReviewerNote,
                Author = string.IsNullOrWhiteSpace(authorUsername) ? "User" : authorUsername,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ReviewNotes.AddAsync(note);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ReviewNoteResponseDto>(note);
        }
    }
}
