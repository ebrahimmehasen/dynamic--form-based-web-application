using AutoMapper;
using StudentRegistry.Application.Constants;
using StudentRegistry.Application.DTOs;
using StudentRegistry.Domain.Entities;
using System;
using System.Linq;

namespace StudentRegistry.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO mapping
            CreateMap<Student, StudentResponseDto>()
                .ForMember(dest => dest.IgGrades, opt => opt.MapFrom(src => src.IgGrades))
                .ForMember(dest => dest.SaudiGrades, opt => opt.MapFrom(src => src.SaudiGrades))
                .ForMember(dest => dest.StandardGrades, opt => opt.MapFrom(src => src.StandardGrades.Where(g => g.GradeLevel == null)))
                .ForMember(dest => dest.KuwaitiGrades, opt => opt.MapFrom(src => src.KuwaitiTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.QatariGrades, opt => opt.MapFrom(src => src.QatariTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.OmaniGrades, opt => opt.MapFrom(src => src.OmaniTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.YemeniGrades, opt => opt.MapFrom(src => src.YemeniTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.BahrainiGrades, opt => opt.MapFrom(src => src.BahrainiTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.KuwaitiTotals, opt => opt.MapFrom(src => src.KuwaitiTotals))
                .ForMember(dest => dest.QatariTotals, opt => opt.MapFrom(src => src.QatariTotals))
                .ForMember(dest => dest.OmaniTotals, opt => opt.MapFrom(src => src.OmaniTotals))
                .ForMember(dest => dest.YemeniTotals, opt => opt.MapFrom(src => src.YemeniTotals))
                .ForMember(dest => dest.BahrainiTotals, opt => opt.MapFrom(src => src.BahrainiTotals))
                .ForMember(dest => dest.PalestinianTotals, opt => opt.MapFrom(src => src.PalestinianTotals))
                .ForMember(dest => dest.OtherTotals, opt => opt.MapFrom(src => src.OtherTotals))
                .ForMember(dest => dest.EgyptianTotals, opt => opt.MapFrom(src => src.EgyptianTotals))
                .ForMember(dest => dest.EgyptianGrades, opt => opt.MapFrom(src => src.EgyptianTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.AzharTotals, opt => opt.MapFrom(src => src.AzharTotals))
                .ForMember(dest => dest.AzharGrades, opt => opt.MapFrom(src => src.AzharTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.EmiratiTotals, opt => opt.MapFrom(src => src.EmiratiTotals))
                .ForMember(dest => dest.EmiratiGrades, opt => opt.MapFrom(src => src.EmiratiTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()))
                .ForMember(dest => dest.AmericanDiplomaTotals, opt => opt.MapFrom(src => src.AmericanDiplomaTotals))
                .ForMember(dest => dest.AmericanDiplomaGrades, opt => opt.MapFrom(src => src.AmericanDiplomaTotals != null ? src.StandardGrades.Where(g => g.GradeLevel != null) : Enumerable.Empty<StandardStudentGrades>()));

            CreateMap<SaudiStudentTotals, SaudiTotalsResponseDto>();
            CreateMap<SaudiStudentGrades, SaudiGradeResponseDto>();

            CreateMap<IgStudentGrades, IgGradesResponseDto>()
                .ForMember(dest => dest.GradeCounts, opt => opt.MapFrom(src => src.Student.IgGradeCounts));

            CreateMap<IgStudentGradeCounts, IgGradeCountResponseDto>();
            CreateMap<StandardStudentGrades, StandardGradeResponseDto>();

            CreateMap<KuwaitiStudentTotals, KuwaitiTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => KuwaitiConstants.Disclaimer))
                .ForMember(dest => dest.SecondAttemptWarning,
                    opt => opt.MapFrom(src => src.HasSecondAttempt ? KuwaitiConstants.SecondAttemptWarning : null));

            CreateMap<StandardStudentGrades, KuwaitiGradeResponseDto>()
                .ForMember(dest => dest.GradeLevel, opt => opt.MapFrom(src => src.GradeLevel!.Value))
                .ForMember(dest => dest.Obtained, opt => opt.MapFrom(src => src.Grade))
                .ForMember(dest => dest.MaxMark, opt => opt.MapFrom(src => src.MaxMark!.Value));

            CreateMap<QatariStudentTotals, QatariTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => KuwaitiConstants.Disclaimer));

            CreateMap<OmaniStudentTotals, OmaniTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => KuwaitiConstants.Disclaimer));

            CreateMap<YemeniStudentTotals, YemeniTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => KuwaitiConstants.Disclaimer));

            CreateMap<BahrainiStudentTotals, BahrainiTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => BahrainiConstants.Disclaimer));

            CreateMap<PalestinianStudentTotals, PalestinianTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => PalestinianConstants.Disclaimer));

            CreateMap<OtherStudentTotals, OtherTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => KuwaitiConstants.Disclaimer));

            CreateMap<EgyptianStudentTotals, EgyptianTotalsResponseDto>();

            CreateMap<AzharStudentTotals, AzharTotalsResponseDto>();

            CreateMap<EmiratiStudentTotals, EmiratiTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => EmiratiConstants.Disclaimer));

            CreateMap<AmericanDiplomaStudentTotals, AmericanDiplomaTotalsResponseDto>()
                .ForMember(dest => dest.Disclaimer, opt => opt.MapFrom(_ => AmericanDiplomaConstants.Disclaimer))
                .ForMember(dest => dest.AdmissionNote, opt => opt.MapFrom(_ => AmericanDiplomaConstants.AdmissionNote));

            CreateMap<StandardStudentGrades, SingleYearSubjectMarkResponseDto>()
                .ForMember(dest => dest.Mark, opt => opt.MapFrom(src => src.Grade));

            // CreateDTO -> Entity mapping
            CreateMap<StudentCreateDto, Student>()
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore()) // Managed by storage service
                .ForMember(dest => dest.SaudiTotals, opt => opt.Ignore())
                .ForMember(dest => dest.SaudiGrades, opt => opt.Ignore())
                .ForMember(dest => dest.IgGrades, opt => opt.Ignore())
                .ForMember(dest => dest.IgGradeCounts, opt => opt.Ignore())
                .ForMember(dest => dest.StandardGrades, opt => opt.Ignore())
                .ForMember(dest => dest.KuwaitiTotals, opt => opt.Ignore())
                .ForMember(dest => dest.QatariTotals, opt => opt.Ignore())
                .ForMember(dest => dest.OmaniTotals, opt => opt.Ignore())
                .ForMember(dest => dest.YemeniTotals, opt => opt.Ignore())
                .ForMember(dest => dest.BahrainiTotals, opt => opt.Ignore())
                .ForMember(dest => dest.PalestinianTotals, opt => opt.Ignore())
                .ForMember(dest => dest.OtherTotals, opt => opt.Ignore())
                .ForMember(dest => dest.EgyptianTotals, opt => opt.Ignore())
                .ForMember(dest => dest.AzharTotals, opt => opt.Ignore())
                .ForMember(dest => dest.EmiratiTotals, opt => opt.Ignore())
                .ForMember(dest => dest.AmericanDiplomaTotals, opt => opt.Ignore());

            // Coefficient is server-computed and validated (Weighted / Achieved) in StudentService,
            // not mapped directly from client input.
            CreateMap<SaudiGradeCreateDto, SaudiStudentGrades>()
                .ForMember(dest => dest.Coefficient, opt => opt.Ignore());

            CreateMap<IgGradeCountCreateDto, IgStudentGradeCounts>();
            
            CreateMap<StandardGradeCreateDto, StandardStudentGrades>()
                .ForMember(dest => dest.Achieved, opt => opt.MapFrom(src => (src.Grade * src.WeightedPercentage) / 100));
        }
    }
}
