namespace StudentRegistry.Application.Editor
{
    // Canonical field-address string used everywhere a field needs to be identified: field_edits /
    // field_comments .FieldName, the click-to-edit UI, and the audit log.
    //   "Student.StudentName"        — a field on the aggregate root
    //   "SaudiTotals.FinalPercentage" — a field on a 1:1 child entity
    //   "SaudiGrades#152.Achieved"    — a field on one row of a 1:N child collection (row Id = 152)
    public static class FieldPath
    {
        public static string Format(string entityGroup, int? entityRowId, string propertyName) =>
            entityRowId.HasValue ? $"{entityGroup}#{entityRowId}.{propertyName}" : $"{entityGroup}.{propertyName}";

        public static bool TryParse(string? fieldPath, out string entityGroup, out int? entityRowId, out string propertyName)
        {
            entityGroup = string.Empty;
            entityRowId = null;
            propertyName = string.Empty;

            if (string.IsNullOrWhiteSpace(fieldPath))
            {
                return false;
            }

            var dotIndex = fieldPath.LastIndexOf('.');
            if (dotIndex <= 0 || dotIndex == fieldPath.Length - 1)
            {
                return false;
            }

            var head = fieldPath[..dotIndex];
            propertyName = fieldPath[(dotIndex + 1)..];

            var hashIndex = head.IndexOf('#');
            if (hashIndex >= 0)
            {
                entityGroup = head[..hashIndex];
                var idPart = head[(hashIndex + 1)..];
                if (!int.TryParse(idPart, out var rowId))
                {
                    return false;
                }
                entityRowId = rowId;
            }
            else
            {
                entityGroup = head;
            }

            return entityGroup.Length > 0 && propertyName.Length > 0;
        }
    }
}
