using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutomatedUITests
{
    public class AutomatedUITests
    {
        //private readonly IWebDriver _driver;
        //public AutomatedUITests() => _driver = new ChromeDriver();
        IWebDriver webDriver = new ChromeDriver(@"D:\Users\granders\Dropbox\VSB Cloud\TSK\Projekty");

        [Test]
        public void Test1()
        {
            //webDriver.Navigate().GoToUrl("https://localhost:7057/Home/TeacherMenu");
            webDriver.Navigate().GoToUrl("https://localhost:7057/Home/ItemTemplate?testNameIdentifier=postest&testNumberIdentifier=i16445870414674424&itemNumberIdentifier=i16445890213918443&itemNameIdentifier=item-5");
         //   webDriver.FindElement(By.LinkText("Spr�va zad�n� test�")).Click();
            Assert.IsNotNull(webDriver.FindElement(By.LinkText("Ulo�it")));
            // Assert.AreEqual(webDriver.FindElement(By.ClassName("teacher-to-index")).Text, "N�vrat do hlavn�ho menuu");
            //   Assert.AreEqual(webDriver.FindElement(By.tit))

            /*webDriver.FindElement(By.Id("subquestion-points")).SendKeys("test");
            webDriver.FindElement(By.LinkText("Ulo�it")).Click();
            Assert.Equals("9", webDriver.FindElement(By.Id("subquestion-points")).Text);*/

        }
    }
}