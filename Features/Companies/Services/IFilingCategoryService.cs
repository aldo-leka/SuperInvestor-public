using SuperInvestor.Features.Companies.Models;

namespace SuperInvestor.Features.Companies.Services;

public interface IFilingCategoryService
{
    List<Filing> CategorizeFilings(Submission submission);
    string GetFilingCategory(string form);
}