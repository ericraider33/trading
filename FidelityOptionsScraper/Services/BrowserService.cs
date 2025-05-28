using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace FidelityOptionsScraper.Services
{
    public class BrowserService : IAsyncDisposable
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;
        private bool _isUsingExistingSession;

        public IPage? CurrentPage => _page;

        /// <summary>
        /// Initializes a new browser instance
        /// </summary>
        /// <param name="useExistingSession">Whether to connect to an existing browser session</param>
        /// <param name="headless">Whether to run in headless mode (invisible browser)</param>
        /// <returns>True if initialization was successful</returns>
        public async Task<bool> InitializeBrowserAsync(bool useExistingSession, bool headless = false)
        {
            try
            {
                _isUsingExistingSession = useExistingSession;
                _playwright = await Playwright.CreateAsync();

                if (useExistingSession)
                {
                    return await AttachToExistingSessionAsync();
                }
                else
                {
                    // Launch a new browser instance
                    _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = headless
                    });

                    _context = await _browser.NewContextAsync();
                    _page = await _context.NewPageAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing browser: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Attaches to an existing browser session
        /// </summary>
        /// <returns>True if attachment was successful</returns>
        private async Task<bool> AttachToExistingSessionAsync()
        {
            try
            {
                Console.WriteLine("\n=== CONNECTING TO EXISTING BROWSER SESSION ===");
                Console.WriteLine("Please follow these steps:");
                Console.WriteLine("1. Open Chrome and navigate to chrome://inspect/#devices");
                Console.WriteLine("2. Click 'Configure...' next to 'Discover network targets'");
                Console.WriteLine("3. Add 'localhost:9222' and click 'Done'");
                Console.WriteLine("4. Open a new Chrome window with the following command:");
                Console.WriteLine("   chrome.exe --remote-debugging-port=9222");
                Console.WriteLine("   (On Mac/Linux: google-chrome --remote-debugging-port=9222)");
                Console.WriteLine("5. Log in to Fidelity in this browser window");
                Console.WriteLine("6. Once logged in, press Enter to continue...");
                Console.ReadLine();

                // Connect to the browser using CDP
                _browser = await _playwright.Chromium.ConnectOverCDPAsync("http://localhost:9222");
                var contexts = _browser.Contexts;
                
                if (contexts.Count == 0)
                {
                    Console.WriteLine("No browser contexts found. Please ensure you have a Chrome window open.");
                    return false;
                }

                _context = contexts[0];
                var pages = _context.Pages;
                
                if (pages.Count == 0)
                {
                    Console.WriteLine("No pages found in the browser context.");
                    return false;
                }

                // Use the first page by default
                _page = pages[0];
                Console.WriteLine($"Connected to existing browser with page: {_page.Url}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to existing browser session: {ex.Message}");
                Console.WriteLine("Please make sure Chrome is running with remote debugging enabled on port 9222.");
                return false;
            }
        }

        /// <summary>
        /// Navigates to a URL
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        /// <returns>True if navigation was successful</returns>
        public async Task<bool> NavigateToAsync(string url)
        {
            if (_page == null)
            {
                Console.WriteLine("Browser not initialized. Call InitializeBrowserAsync first.");
                return false;
            }

            try
            {
                await _page.GotoAsync(url);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to {url}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles Fidelity login if needed
        /// </summary>
        /// <param name="username">Fidelity username</param>
        /// <param name="password">Fidelity password</param>
        /// <returns>True if login was successful or already logged in</returns>
        public async Task<bool> LoginIfNeededAsync(string username, string password)
        {
            if (_page == null)
            {
                Console.WriteLine("Browser not initialized. Call InitializeBrowserAsync first.");
                return false;
            }

            try
            {
                // Check if we're already on a page that indicates we're logged in
                string currentUrl = _page.Url;
                if (currentUrl.Contains("digital.fidelity.com") && !currentUrl.Contains("login"))
                {
                    Console.WriteLine("Already logged in to Fidelity.");
                    return true;
                }

                // Navigate to login page if not already there
                if (!currentUrl.Contains("login.fidelity.com"))
                {
                    await _page.GotoAsync("https://login.fidelity.com/ftgw/Fas/Fidelity/RtlCust/Login/Init");
                }

                // Fill in username and password
                await _page.FillAsync("#userId-input", username);
                await _page.FillAsync("#password", password);
                
                // Click login button
                await _page.ClickAsync("#fs-login-button");
                
                // Wait for navigation to complete
                await _page.WaitForURLAsync("**/digital.fidelity.com/**");
                
                // Check if login was successful
                if (_page.Url.Contains("digital.fidelity.com"))
                {
                    Console.WriteLine("Successfully logged in to Fidelity.");
                    return true;
                }
                else
                {
                    Console.WriteLine("Login failed. Please check your credentials.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disposes of browser resources
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_browser != null && !_isUsingExistingSession)
            {
                await _browser.CloseAsync();
            }
            
            _playwright?.Dispose();
        }
    }
}
