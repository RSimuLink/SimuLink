using UglyToad.PdfPig;

namespace RocheSimuLink.Him
{
    /// <summary>
    /// Reads an x800 Data Manager Host Interface Manual PDF into text using
    /// PdfPig (a managed library, so this runs on any platform and stays
    /// unit-testable). Exposes per-page text because the manual's assay blocks
    /// are page-scoped, which makes per-page parsing far more robust than
    /// scanning one giant string.
    ///
    /// Pages are reconstructed from PdfPig's word list joined by spaces rather
    /// than the raw <c>page.Text</c>: the manual's tables otherwise glue
    /// adjacent cells together (e.g. "PLASMAPLAS^plasma^HL70487850"), and the
    /// word-joined form keeps cell boundaries intact.
    /// </summary>
    public static class HimPdfReader
    {
        /// <summary>Reads each page's text from the PDF at <paramref name="path"/>.</summary>
        public static IReadOnlyList<string> ReadPages(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);

            using var document = PdfDocument.Open(path);
            return ReadPages(document);
        }

        /// <summary>Reads each page's text from an open PDF document.</summary>
        public static IReadOnlyList<string> ReadPages(PdfDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var pages = new List<string>(document.NumberOfPages);
            for (var i = 1; i <= document.NumberOfPages; i++)
            {
                var page = document.GetPage(i);
                pages.Add(string.Join(' ', page.GetWords().Select(w => w.Text)));
            }

            return pages;
        }
    }
}
