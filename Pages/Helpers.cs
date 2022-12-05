using System.Globalization;
using BTCPayServer.Lightning;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BTCPayServer.Plugins.PodServer.Pages;

public static class Helpers
{
    public static string Sats(LightMoney amount) => $"{Math.Round(amount.ToUnit(LightMoneyUnit.Satoshi))} sats";
    
    public static IEnumerable<SelectListItem> LanguageOptions => CultureInfo.GetCultures(CultureTypes.NeutralCultures)
        .Select(ci => ci.TwoLetterISOLanguageName == "iv"
            ? new SelectListItem(string.Empty, string.Empty)
            : new SelectListItem(ci.DisplayName, ci.TwoLetterISOLanguageName));

    public static IEnumerable<SelectListItem> MediumOptions => new[]
    {
        new SelectListItem { Text = "Audiobook", Value = "audiobook" },
        new SelectListItem { Text = "Blog", Value = "blog" },
        new SelectListItem { Text = "Film", Value = "film" },
        new SelectListItem { Text = "Music", Value = "music" },
        new SelectListItem { Text = "Newsletter", Value = "newsletter" },
        new SelectListItem { Text = "Podcast", Value = "podcast" },
        new SelectListItem { Text = "Video", Value = "video" }
    };
    
    public static IEnumerable<SelectListItem> CategoryOptions => new[]
    {
        new SelectListItem(string.Empty, string.Empty),
        
        new SelectListItem { Text = "Arts", Value = "Arts", Group = _categoryGroupArts },
        new SelectListItem { Text = "Books", Value = "Books", Group = _categoryGroupArts },
        new SelectListItem { Text = "Design", Value = "Design", Group = _categoryGroupArts },
        new SelectListItem { Text = "Fashion & Beauty", Value = "Fashion & Beauty", Group = _categoryGroupArts },
        new SelectListItem { Text = "Food", Value = "Food", Group = _categoryGroupArts },
        new SelectListItem { Text = "Performing Arts", Value = "Performing Arts", Group = _categoryGroupArts },
        new SelectListItem { Text = "Visual Arts", Value = "Visual Arts", Group = _categoryGroupArts },
        
        new SelectListItem { Text = "Business", Value = "Business", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Careers", Value = "Careers", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Entrepreneurship", Value = "Entrepreneurship", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Investing", Value = "Investing", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Management", Value = "Management", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Marketing", Value = "Marketing", Group = _categoryGroupBusiness },
        new SelectListItem { Text = "Non-Profit", Value = "Non-Profit", Group = _categoryGroupBusiness },
        
        new SelectListItem { Text = "Comedy", Value = "Comedy", Group = _categoryGroupComedy },
        new SelectListItem { Text = "Comedy Interviews", Value = "Comedy Interviews", Group = _categoryGroupComedy },
        new SelectListItem { Text = "Improv", Value = "Improv", Group = _categoryGroupComedy },
        new SelectListItem { Text = "Stand-Up", Value = "Stand-Up", Group = _categoryGroupComedy },
        
        new SelectListItem { Text = "Education", Value = "Education", Group = _categoryGroupEducation },
        new SelectListItem { Text = "Courses", Value = "Courses", Group = _categoryGroupEducation },
        new SelectListItem { Text = "How To", Value = "How To", Group = _categoryGroupEducation },
        new SelectListItem { Text = "Language Learning", Value = "Language Learning", Group = _categoryGroupEducation },
        new SelectListItem { Text = "Self-Improvement", Value = "Self-Improvement", Group = _categoryGroupEducation },
        
        new SelectListItem { Text = "Fiction", Value = "Fiction", Group = _categoryGroupFiction },
        new SelectListItem { Text = "Comedy Fiction", Value = "Comedy Fiction", Group = _categoryGroupFiction },
        new SelectListItem { Text = "Drama", Value = "Drama", Group = _categoryGroupFiction },
        new SelectListItem { Text = "Science Fiction", Value = "Science Fiction", Group = _categoryGroupFiction },
        
        new SelectListItem { Text = "Government", Value = "Government", Group = _categoryGroupGovernment },
        
        new SelectListItem { Text = "History", Value = "History", Group = _categoryGroupHistory },
        
        new SelectListItem { Text = "Health & Fitness", Value = "Health & Fitness", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Alternative Health", Value = "Alternative Health", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Fitness", Value = "Fitness", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Medicine", Value = "Medicine", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Mental Health", Value = "Mental Health", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Nutrition", Value = "Nutrition", Group = _categoryGroupHealth },
        new SelectListItem { Text = "Sexuality", Value = "Sexuality", Group = _categoryGroupHealth },
        
        new SelectListItem { Text = "Kids & Family", Value = "Kids & Family", Group = _categoryGroupKids },
        new SelectListItem { Text = "Education for Kids", Value = "Education for Kids", Group = _categoryGroupKids },
        new SelectListItem { Text = "Parenting", Value = "Parenting", Group = _categoryGroupKids },
        new SelectListItem { Text = "Pets & Animals", Value = "Pets & Animals", Group = _categoryGroupKids },
        new SelectListItem { Text = "Stories for Kids", Value = "Stories for Kids", Group = _categoryGroupKids },
        
        new SelectListItem { Text = "Leisure", Value = "Leisure", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Animation & Manga", Value = "Animation & Manga", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Automotive", Value = "Automotive", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Aviation", Value = "Aviation", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Crafts", Value = "Crafts", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Games", Value = "Games", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Hobbies", Value = "Hobbies", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Home & Garden", Value = "Home & Garden", Group = _categoryGroupLeisure },
        new SelectListItem { Text = "Video Games", Value = "Video Games", Group = _categoryGroupLeisure },

        new SelectListItem { Text = "Music", Value = "Music", Group = _categoryGroupMusic },
        new SelectListItem { Text = "Music Commentary", Value = "Music Commentary", Group = _categoryGroupMusic },
        new SelectListItem { Text = "Music History", Value = "Music History", Group = _categoryGroupMusic },
        new SelectListItem { Text = "Music Interviews", Value = "Music Interviews", Group = _categoryGroupMusic },
        
        new SelectListItem { Text = "News", Value = "News", Group = _categoryGroupNews },
        new SelectListItem { Text = "Business News", Value = "Business News", Group = _categoryGroupNews },
        new SelectListItem { Text = "Daily News", Value = "Daily News", Group = _categoryGroupNews },
        new SelectListItem { Text = "Entertainment News", Value = "Entertainment News", Group = _categoryGroupNews },
        new SelectListItem { Text = "News Commentary", Value = "News Commentary", Group = _categoryGroupNews },
        new SelectListItem { Text = "Politics", Value = "Politics", Group = _categoryGroupNews },
        new SelectListItem { Text = "Sports News", Value = "Sports News", Group = _categoryGroupNews },
        new SelectListItem { Text = "Tech News", Value = "Tech News", Group = _categoryGroupNews },

        new SelectListItem { Text = "Religion & Spirituality", Value = "Religion & Spirituality", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Buddhism", Value = "Buddhism", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Christianity", Value = "Christianity", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Hinduism", Value = "Hinduism", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Islam", Value = "Islam", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Judaism", Value = "Judaism", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Religion", Value = "Religion", Group = _categoryGroupReligion },
        new SelectListItem { Text = "Spirituality", Value = "Spirituality", Group = _categoryGroupReligion },

        new SelectListItem { Text = "Science", Value = "Science", Group = _categoryGroupScience },
        new SelectListItem { Text = "Astronomy", Value = "Astronomy", Group = _categoryGroupScience },
        new SelectListItem { Text = "Chemistry", Value = "Chemistry", Group = _categoryGroupScience },
        new SelectListItem { Text = "Earth Sciences", Value = "Earth Sciences", Group = _categoryGroupScience },
        new SelectListItem { Text = "Life Sciences", Value = "Life Sciences", Group = _categoryGroupScience },
        new SelectListItem { Text = "Mathematics", Value = "Mathematics", Group = _categoryGroupScience },
        new SelectListItem { Text = "Natural Sciences", Value = "Natural Sciences", Group = _categoryGroupScience },
        new SelectListItem { Text = "Nature", Value = "Nature", Group = _categoryGroupScience },
        new SelectListItem { Text = "Physics", Value = "Physics", Group = _categoryGroupScience },
        new SelectListItem { Text = "Social Sciences", Value = "Social Sciences", Group = _categoryGroupScience },
        
        new SelectListItem { Text = "Society & Culture", Value = "Society & Culture", Group = _categoryGroupSociety },
        new SelectListItem { Text = "Documentary", Value = "Documentary", Group = _categoryGroupSociety },
        new SelectListItem { Text = "Personal Journals", Value = "Personal Journals", Group = _categoryGroupSociety },
        new SelectListItem { Text = "Philosophy", Value = "Philosophy", Group = _categoryGroupSociety },
        new SelectListItem { Text = "Places & Travel", Value = "Places & Travel", Group = _categoryGroupSociety },
        new SelectListItem { Text = "Relationships", Value = "Relationships", Group = _categoryGroupSociety },
        
        new SelectListItem { Text = "Sports", Value = "Sports", Group = _categoryGroupSports },
        new SelectListItem { Text = "Baseball", Value = "Baseball", Group = _categoryGroupSports },
        new SelectListItem { Text = "Basketball", Value = "Basketball", Group = _categoryGroupSports },
        new SelectListItem { Text = "Cricket", Value = "Cricket", Group = _categoryGroupSports },
        new SelectListItem { Text = "Fantasy Sports", Value = "Fantasy Sports", Group = _categoryGroupSports },
        new SelectListItem { Text = "Football", Value = "Football", Group = _categoryGroupSports },
        new SelectListItem { Text = "Golf", Value = "Golf", Group = _categoryGroupSports },
        new SelectListItem { Text = "Hockey", Value = "Hockey", Group = _categoryGroupSports },
        new SelectListItem { Text = "Rugby", Value = "Rugby", Group = _categoryGroupSports },
        new SelectListItem { Text = "Running", Value = "Running", Group = _categoryGroupSports },
        new SelectListItem { Text = "Soccer", Value = "Soccer", Group = _categoryGroupSports },
        new SelectListItem { Text = "Swimming", Value = "Swimming", Group = _categoryGroupSports },
        new SelectListItem { Text = "Tennis", Value = "Tennis", Group = _categoryGroupSports },
        new SelectListItem { Text = "Volleyball", Value = "Volleyball", Group = _categoryGroupSports },
        new SelectListItem { Text = "Wilderness", Value = "Wilderness", Group = _categoryGroupSports },
        new SelectListItem { Text = "Wrestling", Value = "Wrestling", Group = _categoryGroupSports },
        
        new SelectListItem { Text = "Technology", Value = "Technology", Group = _categoryGroupTechnology },
        
        new SelectListItem { Text = "True Crime", Value = "True Crime", Group = _categoryGroupTrueCrime },
        
        new SelectListItem { Text = "TV & Film", Value = "TV & Film", Group = _categoryGroupTv },
        new SelectListItem { Text = "After Shows", Value = "After Shows", Group = _categoryGroupTv },
        new SelectListItem { Text = "Film History", Value = "Film History", Group = _categoryGroupTv },
        new SelectListItem { Text = "Film Interviews", Value = "Film Interviews", Group = _categoryGroupTv },
        new SelectListItem { Text = "Film Reviews", Value = "Film Reviews", Group = _categoryGroupTv },
        new SelectListItem { Text = "TV Reviews", Value = "TV Reviews", Group = _categoryGroupTv }
    };
        
    private static readonly SelectListGroup _categoryGroupArts = new() { Name = "Arts" };
    private static readonly SelectListGroup _categoryGroupBusiness = new() { Name = "Business" };
    private static readonly SelectListGroup _categoryGroupComedy = new() { Name = "Comedy" };
    private static readonly SelectListGroup _categoryGroupEducation = new() { Name = "Education" };
    private static readonly SelectListGroup _categoryGroupFiction = new() { Name = "Fiction" };
    private static readonly SelectListGroup _categoryGroupGovernment = new() { Name = "Government" };
    private static readonly SelectListGroup _categoryGroupHistory = new() { Name = "History" };
    private static readonly SelectListGroup _categoryGroupHealth = new() { Name = "Health & Fitness" };
    private static readonly SelectListGroup _categoryGroupKids = new() { Name = "Kids & Family" };
    private static readonly SelectListGroup _categoryGroupLeisure = new() { Name = "Leisure" };
    private static readonly SelectListGroup _categoryGroupMusic = new() { Name = "Music" };
    private static readonly SelectListGroup _categoryGroupNews = new() { Name = "News" };
    private static readonly SelectListGroup _categoryGroupReligion = new() { Name = "Religion & Spirituality" };
    private static readonly SelectListGroup _categoryGroupScience = new() { Name = "Science" };
    private static readonly SelectListGroup _categoryGroupSociety = new() { Name = "Society & Culture" };
    private static readonly SelectListGroup _categoryGroupSports = new() { Name = "Sports" };
    private static readonly SelectListGroup _categoryGroupTechnology = new() { Name = "Technology" };
    private static readonly SelectListGroup _categoryGroupTrueCrime = new() { Name = "True Crime" };
    private static readonly SelectListGroup _categoryGroupTv = new() { Name = "TV & Film" };
}
