using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Drawing;
using System.Text;


class FacebookScraper
{
    private static IWebDriver driver;
    private List<string> posts;

    public FacebookScraper()
    {
        var options = new ChromeOptions();

        options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");
        options.AddExcludedArgument("enable-automation");
        options.AddAdditionalOption("useAutomationExtension", false);
        options.AddArgument(@"user-data-dir=C:\Users\PC\AppData\Local\Google\Chrome\User Data\");
        options.AddArgument("profile-directory=Profile 11");

        driver = new ChromeDriver(options);

        int screenWidth = 1920;
        int screenHeight = 1080;
        driver.Manage().Window.Size = new Size(screenWidth / 2, screenHeight); // Nửa màn hình
        driver.Manage().Window.Position = new Point(screenWidth / 2, 0);
        posts = new List<string>();
    }

    static void Main(string[] args)
    {
        var scraper = new FacebookScraper();
        driver.Navigate().GoToUrl("https://www.facebook.com/");

        //scraper.Login();
        scraper.GoToGroup();

        //Encoding for console
        Console.OutputEncoding = Encoding.UTF8;
        scraper.GetPosts2_GetFullContent();

        Console.WriteLine("----------------------------------------------");
        Console.WriteLine("Posts found: " + scraper.posts.Count);
    }

    public void Login()
    {
        // Log in to Facebook
        driver.FindElement(By.Id("email")).SendKeys("phuongcq1289");
        driver.FindElement(By.Id("pass")).SendKeys("Lavender@8");
        driver.FindElement(By.Id("pass")).SendKeys(Keys.Enter);

        // Wait for login to complete
        Thread.Sleep(5000);



    }

    public void GoToGroup()
    {

        // Navigate to the Facebook group
        driver.Navigate().GoToUrl("https://www.facebook.com/groups/1826285257467256/");
        Thread.Sleep(5000);
    }
    public void GetPosts1()
    {
        for (int i = 0; i < 10; i++) // Adjust for number of scrolls
        {
            // Find post elements
            var postElements = driver.FindElements(By.CssSelector("div[data-ad-preview='message']"));
            foreach (var postElement in postElements)
            {
                string postText = postElement.Text;
                if (!posts.Contains(postText))
                {
                    posts.Add(postText);
                    Console.WriteLine("Post Found: " + postText);
                    Console.WriteLine("---------------------------------------------------");
                }
            }

            // Scroll down
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            Thread.Sleep(3000);
        }
    }
    public void GetPosts2_GetFullContent()
    {
        // Danh sách để lưu nội dung bài viết
        var postContents = new HashSet<string>(); // Sử dụng HashSet để loại bỏ bài viết trùng lặp
        var postIds = new HashSet<string>(); // Lưu ID của các bài viết để kiểm tra trùng lặp

        for (int i = 0; i < 10; i++) // Adjust for number of scrolls
        {
            var posts = driver.FindElements(By.XPath("//div[@data-ad-rendering-role='story_message']")).ToList();

            // Kiểm tra bài viết mới
            foreach (var post in posts)
            {
                try
                {
                    Thread.Sleep(2000);
                    // Get post ID using the aria-labelledby attribute
                    var postIdElement = post.FindElement(By.XPath(".//ancestor::div[@aria-labelledby]"));
                    var postId = postIdElement.GetDomAttribute("aria-labelledby");

                    if (string.IsNullOrEmpty(postId) || postIds.Contains(postId))
                    {
                        continue; // Skip posts that have already been processed
                    }

                    // Add the post ID to the HashSet to avoid duplicates
                    postIds.Add(postId);

                    // Extract the post content before click see more(using a selector for post text)
                    var content = post.Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        postContents.Add(content); // Add the content to HashSet
                        Console.WriteLine("Post Found in brief: " + postContents.Count);
                        Console.WriteLine(content);
                        Console.WriteLine("---------------------------------------------------");
                    }

                    // Find and click the "See More" button if available
                    var seeMoreButtons = post.FindElements(By.XPath(".//div[contains(text(), 'Xem thêm') or contains(text(), 'See More')]"));

                    if (seeMoreButtons.Count > 0) // If "See More" button is found
                    {
                        // Scroll the element into view with an offset
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", seeMoreButtons[0]);
                        // Inside the GetPosts2_GetFullContent method, replace the problematic line with the following code
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        var seeMoreButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath(".//div[contains(text(), 'Xem thêm') or contains(text(), 'See More')]")));
                        Thread.Sleep(1000);
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", seeMoreButton);
                        Thread.Sleep(1000); // Wait for the expanded content to load
                    }

                    // Extract the post content (using a selector for post text)
                    Thread.Sleep(2000);
                    content = post.Text;
                    if (!string.IsNullOrEmpty(content))
                    {
                        //postContents.Add(content); // Add the content to HashSet
                        Console.WriteLine("Post Found: " + postContents.Count);
                        Console.WriteLine(content);
                        Console.WriteLine("---------------------------------------------------");
                    }
                }
                catch (NoSuchElementException)
                {
                    // Bỏ qua nếu không tìm thấy nội dung bài viết hoặc nút "Xem thêm"
                }
                catch (StaleElementReferenceException)
                {
                    // Re-locate the element and retry
                    continue;
                }
            }

           // Scroll down
           ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
            Thread.Sleep(3000);
        }
    }

}
