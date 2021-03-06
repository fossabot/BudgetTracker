﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTracker.Controllers.ViewModels.Table;
using BudgetTracker.Model;

namespace BudgetTracker.Controllers.ViewModels.Widgets
{
    public class DeltaWidgetViewModel : WidgetViewModel
    {
        public DeltaWidgetViewModel(WidgetModel model, ObjectRepository repository) : base(model,
            new DeltaWidgetSettings(model.Properties.ToDictionary(v => v.Key, v => v.Value)))
        {
            DeltaWidgetSettings = (DeltaWidgetSettings) Settings;

            var table = TableViewModel.GetCachedViewModel(true, false, false, repository);

            var col = repository.Set<MoneyColumnMetadataModel>().First(v =>
                v.Provider == DeltaWidgetSettings.ProviderName &&
                (v.AccountName == DeltaWidgetSettings.AccountName ||
                 v.UserFriendlyName == DeltaWidgetSettings.AccountName));

            AddDelta(col, table, 1, "24ч");
            AddDelta(col, table, 2, "48ч");
            AddDelta(col, table, 7, "1н");
            AddDelta(col, table, 24, "1м");
        }

        private void AddDelta(MoneyColumnMetadataModel col, TableViewModel table, int daysDiff, string name)
        {
            var today = table.Values.OrderByDescending(v => v.When).FirstOrDefault();
            var baseSet = table.Values.OrderByDescending(v => v.When).FirstOrDefault(v => v.When.AddDays(daysDiff) < DateTime.Now);
            
            var todayValue = today?.Cells.FirstOrDefault(v => v.Column == col);
            var baseSetValue = baseSet?.Cells.FirstOrDefault(v => v.Column == col);

            if (todayValue != null && baseSetValue != null)
            {
                Ccy = Ccy ?? todayValue.Ccy;

                IncompleteData |= todayValue.FailedToResolve.Concat(baseSetValue.FailedToResolve).Any();
                
                var dT = todayValue.Value - baseSetValue.Value;

                if (dT != null && !double.IsNaN(dT.Value) && !double.IsInfinity(dT.Value))
                {
                    Deltas.Add((name, dT.Value));
                }
                else
                {
                    IncompleteData = true;
                }
            }
        }

        public DeltaWidgetSettings DeltaWidgetSettings { get; set; }
        public override string TemplateName => WidgetExtensions.AsPath("~/Views/Widget/Widgets/DeltaWidget.cshtml");
        public override int Columns => 4;
        public bool IncompleteData { get; private set; }
        
        public string Ccy { get; private set; }
        
        public List<(string, double)> Deltas { get; } = new List<(string, double)>(); 
    }
}