namespace MySimpleNetApp.UiTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PrivacyPageTests : PageTest
{
    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5033";

    [Test]
    public async Task PrivacyPage_WhenOpened_ShowsPolicySections()
    {
        await Page.GotoAsync($"{BaseUrl}/Home/Privacy");

        await Expect(Page).ToHaveTitleAsync(new Regex("Privacy Policy"));

        foreach (var heading in new[] { "What we collect", "Cookies", "Third parties", "Your rights" })
        {
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = heading }))
                .ToBeVisibleAsync();
        }
    }
}
