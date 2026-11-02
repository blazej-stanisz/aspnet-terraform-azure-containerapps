namespace MySimpleNetApp.UiTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class HomePageTests : PageTest
{
    // Port z profilu "http" w WebUI/Properties/launchSettings.json. W CI TEST_BASE_URL wskazuje na
    // aplikację uruchomioną przez workflow.
    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? "http://localhost:5033";

    [Test]
    public async Task HomePage_WhenOpened_ShowsProjectSummary()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        await Expect(Page).ToHaveTitleAsync(new Regex("Home"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "What this project is" }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByText("Azure Container Apps")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Navigation_WhenPrivacyLinkClicked_OpensPrivacyPage()
    {
        await Page.GotoAsync($"{BaseUrl}/");

        // Dwa linki "Privacy" (nawigacja i stopka), bierzemy pierwszy.
        await Page.GetByRole(AriaRole.Link, new() { Name = "Privacy" }).First.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/Home/Privacy$"));
    }
}
