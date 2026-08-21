using Microsoft.Playwright;

namespace HRMS.Web.BusinessLayer
{
    public class SalarySlipPdfService
    {
        private readonly IConfiguration _configuration;

        public SalarySlipPdfService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<byte[]> GeneratePdfAsync(string html)
        {
            using var playwright = await Playwright.CreateAsync();

            var browserOptions = new BrowserTypeLaunchOptions
            {
                Headless = true
            };

            var browserPath = _configuration[
                "Playwright:ExecutablePath"
            ];

            if (!string.IsNullOrWhiteSpace(browserPath))
            {
                browserOptions.ExecutablePath = browserPath;
            }

            await using var browser =
                await playwright.Chromium.LaunchAsync(browserOptions);

            var page = await browser.NewPageAsync();

            await page.SetContentAsync(
                html,
                new PageSetContentOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                }
            );

            await page.EvaluateAsync(
                @"async () => {
                    const images = Array.from(document.images);

                    await Promise.all(
                        images.map(img => {
                            if (img.complete)
                                return Promise.resolve();

                            return new Promise(resolve => {
                                img.onload = resolve;
                                img.onerror = resolve;
                            });
                        })
                    );
                }"
            );

            await page.WaitForTimeoutAsync(300);

            return await page.PdfAsync(
                new PagePdfOptions
                {
                    Format = "A4",
                    PrintBackground = true,
                    PreferCSSPageSize = true,
                    Margin = new Margin
                    {
                        Top = "0",
                        Bottom = "0",
                        Left = "0",
                        Right = "0"
                    }
                }
            );
        }
    }
}