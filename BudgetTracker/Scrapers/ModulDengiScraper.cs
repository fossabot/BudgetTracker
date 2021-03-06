﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace BudgetTracker.Scrapers
{
    internal class ModulDengiScraper : GenericScraper
    {
        public override string ProviderName => "МодульДеньги";

        public override IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver)
        {
            driver.Navigate().GoToUrl("https://cabinet.moduldengi.ru/#/");

            var auth = GetElement(driver, By.ClassName("auth"));
            var phone = auth.FindElement(By.TagName("input"));
            phone.Click();
            foreach (var ch in configuration.Login)
            {
                driver.Keyboard.SendKeys(ch.ToString());
                Thread.Sleep(100);
            }

            driver.Keyboard.PressKey(Keys.Enter);
            Thread.Sleep(1000);

            var inputs = auth.FindElements(By.TagName("input"));
            var pass = inputs.First(v => v.GetAttribute("id") != phone.GetAttribute("id"));
            pass.Click();
            driver.Keyboard.SendKeys(configuration.Password);
            driver.Keyboard.PressKey(Keys.Return);

            var row = GetElement(driver, By.ClassName("balances-row"));

            var cells = row.FindElements(By.ClassName("balances-item"));

            var result = new List<MoneyStateModel>();

            foreach (var cell in cells)
            {
                var label = cell.FindElement(By.ClassName("property")).Text;
                var value = cell.FindElement(By.ClassName("value")).Text;

                var cleanValueString = new string(value.Where(v => char.IsDigit(v) || v == '.').ToArray());

                var cleanValue = double.Parse(cleanValueString, new NumberFormatInfo
                {
                    NumberDecimalSeparator = "."
                });

                result.Add(Money(label, cleanValue, "RUB"));
            }

            return result;
        }
    }
}