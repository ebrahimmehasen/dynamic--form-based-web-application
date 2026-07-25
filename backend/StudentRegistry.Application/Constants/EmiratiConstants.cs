namespace StudentRegistry.Application.Constants
{
    // "الشهادة الإماراتية" — currently a single track/path (no path selection needed in the UI
    // yet). Structured so a second track can be added later without a rewrite: Tracks stays a
    // proper array (today with one entry), and a future GetSubjectSet(track) would grow a switch
    // the same way BahrainiConstants.GetTrackSubjects does — call sites in the validator/service
    // would not need to change shape, only the lookup itself.
    // Shared single-year-fixed-total rules (max mark per subject) live in SingleYearFixedTotalConstants.
    // Referenced by ConfigController (API), StudentCreateDtoValidator, and StudentService.
    public static class EmiratiConstants
    {
        public const string SingleTrack = "المسار العام";

        public static readonly string[] Tracks = { SingleTrack };

        // Core (mandatory) subjects — exactly 5, always required.
        public static readonly string[] CoreSubjects =
        {
            "اللغة العربية", "الدراسات الاجتماعية والتربية الأخلاقية", "اللغة الإنجليزية",
            "الرياضيات", "الفيزياء (ADV.PHY.M.102-A)"
        };

        // Optional subjects — counted (added to both the numerator and the denominator) only if
        // the student actually submits a mark for them. Always optional, even for medical Wish
        // colleges — the medical warning only recommends entering them, it never makes them required.
        public static readonly string[] OptionalSubjects =
        {
            "الكيمياء (ADV.CHM.C.101)", "العلوم الصحية", "الأحياء"
        };

        // Wish colleges where the medical-subjects warning applies — mirrors the medical subset of
        // WishConstants.Colleges.
        public static readonly string[] MedicalWishColleges =
        {
            WishConstants.HumanMedicine, WishConstants.Dentistry, WishConstants.Pharmacy, WishConstants.Nursing
        };

        public const string MedicalWishWarning =
            "عند اختيار أي رغبة من الرغبات الطبية، يجب إدخال الدرجات التفصيلية للمواد المضافة للمجموع (الموجودة في شهادتك) إن وُجدت، لأنها مطلب أساسي وإلزامي في التنسيق المصري للقبول بالكليات الطبية، ولن يتم النظر في رغبتك دون إدخالها.";

        public const string Disclaimer =
            "هذه النتيجة تقديرية ويجب تأكيدها رسمياً من مكتب تنسيق القبول بالجامعات والمعاهد المصرية.";
    }
}
