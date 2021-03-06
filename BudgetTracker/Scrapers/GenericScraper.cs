﻿using System;
using System.Collections.Generic;
using BudgetTracker.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BudgetTracker.Scrapers
{
    public abstract class GenericScraper
    {
        public abstract string ProviderName { get; }

        public abstract IList<MoneyStateModel> Scrape(ScraperConfigurationModel configuration, ChromeDriver driver);
        
        protected IWebElement GetElement(ChromeDriver driver, By currencySpan)
        {
            var wt = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var amountWait = wt.Until(d => d.FindElement(currencySpan));
            return amountWait;
        }

        protected MoneyStateModel Money(string account, double amount, string ccy) => new MoneyStateModel
        {
            Provider = ProviderName,
            AccountName = account,
            When = DateTime.UtcNow.Date,
            Ccy = ccy,
            Amount = amount
        };

        protected PaymentModel Statement(DateTime when, string account, string what, double amount, string ccy, string statementReference) =>
            new PaymentModel(ProviderName, account, when, what, amount, ccy, statementReference);
    }
}