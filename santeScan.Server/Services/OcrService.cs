using Tesseract;

namespace santeScan.Server.Services
{
    public class OcrService
    {
        public string ExtraireTexteAnalyse(string cheminImage)
        {
            // "./tessdata" est le chemin vers votre dossier de langue, "fra" pour français
            using (var engine = new TesseractEngine(@"./tessdata", "fra", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(cheminImage))
                {
                    using (var page = engine.Process(img))
                    {
                        return page.GetText(); // Renvoie le texte brut extrait de la prise de sang
                    }
                }
            }
        }
    }
}
