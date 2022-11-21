using AllUpBack.DAL.Entities;

namespace AllUpBack.ViewModels
{
    public class LanguageViewModel
    {
        public List<Language> Languages { get; set; }= new List<Language>();
        public Language SelectedLanguage { get; set; }= new Language();
    }
}
