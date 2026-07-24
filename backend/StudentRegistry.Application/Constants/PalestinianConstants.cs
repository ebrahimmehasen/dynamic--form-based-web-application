namespace StudentRegistry.Application.Constants
{
    // Palestinian Tawjihi rules — percentage-in only, no subjects. Referenced by ConfigController
    // (API), StudentCreateDtoValidator, and StudentService.
    public static class PalestinianConstants
    {
        public const string ScientificBranch = "علمي";
        public const string LiteraryBranch = "أدبي";

        // §1.3 — Tawjihi's official equivalence body differs from the shared disclaimer's wording
        // (مكتب التنسيق), so this certificate gets its own override rather than changing the shared
        // text for every other certificate.
        public const string Disclaimer =
            "النتيجة تقديرية استرشادية فقط، والمعادلة الرسمية النهائية تتم من خلال المجلس الأعلى للجامعات.";
    }
}
