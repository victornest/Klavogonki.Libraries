using Klavogonki;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Klavogonki
{
    public class GenericTableExcelWriter : IGenericTableExcelWriter
    {
        public Range WriteGenericData(List<Player> players, TableDataSettings settings, Worksheet sh)
        {
            players.Sort(settings.Comparer);

            const int initialColumn = 2; // 1-based
            int currentColumn = initialColumn;
            int racesCreated = players[0].RacesCreated;
            int lastColumn = initialColumn + settings.Columns.Count + racesCreated + (settings.AdditionalColumns?.Count ?? 0) - 1;
            const int headerRow = 2;
            int lastRow = headerRow + players.Count;

            //Выравнивание
            var alltable = sh.Range[sh.Cells[headerRow, initialColumn],
                                    sh.Cells[lastRow, lastColumn]] as Range;
            alltable.HorizontalAlignment = XlHAlign.xlHAlignCenter;      
            foreach (var column in settings.Columns)
            {
                FillColumn(column);
                currentColumn++;
            }
      
            for (int r = 0; r < racesCreated; r++)
            {
                sh.Cells[headerRow, currentColumn + r].Value2 = r + 1;
            }

            var header = sh.Range[sh.Cells[headerRow, initialColumn],  
                                  sh.Cells[headerRow, lastColumn]];
            header.Font.Bold = true;
            header.Interior.Color = ColorTranslator.FromHtml("#c0c0c0");
            header.VerticalAlignment = XlVAlign.xlVAlignCenter;
            header.WrapText = true;

            int currentPlayer = 0; // 0-based
            foreach (var player in players)
            {
                foreach (var r in player.Results)
                {
                    var cell = sh.Cells[headerRow + 1 + currentPlayer, currentColumn + r.Key - 1];
                    Result result = r.Value;
                    double? value = settings.RaceValue?.Invoke(result);
                    if (value != null)cell.Value2 = value;

                    if (settings.IsBold?.Invoke(result) == true)
                        cell.Font.Bold = true;

                    if (settings.IsGray?.Invoke(result) == true)
                        cell.Font.Color = ColorTranslator.FromHtml("#888888");

                    if (settings.IsRecord?.Invoke(result) == true)
                        cell.Font.ColorIndex = 3;
                }
                
                (sh.Range[sh.Cells[headerRow + currentPlayer + 1, initialColumn],
                          sh.Cells[headerRow + currentPlayer + 1, lastColumn]])
                          .Interior.ColorIndex = player.Rank.BackColorIndex;

                currentPlayer++;
            }

            (sh.Range[sh.Cells[headerRow, currentColumn],
                      sh.Cells[headerRow, currentColumn + racesCreated - 1]])
                .Columns.ColumnWidth = settings.ResultColumnWidth;


            currentColumn += racesCreated;
            foreach (var column in settings.AdditionalColumns)
            {
                FillColumn(column);
                currentColumn++;
            }

            //внутренние границы
            alltable.Borders[XlBordersIndex.xlInsideVertical].LineStyle = XlLineStyle.xlContinuous;
            alltable.Borders[XlBordersIndex.xlInsideHorizontal].LineStyle = XlLineStyle.xlContinuous;

            //внешние толстые границы:
            alltable.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
            alltable.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;
            alltable.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
            alltable.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;

            return alltable;

            void FillColumn(ColumnDataSettings columnSettings)
            {
                var columnRange = (sh.Range[sh.Cells[headerRow + 1, currentColumn],
                            sh.Cells[lastRow, currentColumn]]);
                sh.Cells[headerRow, currentColumn].Value2 = columnSettings.Name;

                int _currentPlayer = 0; // 0-based
                foreach (var player in players)
                {
                    dynamic value = columnSettings.Value != null ?
                        columnSettings.Value?.Invoke(player) : _currentPlayer + 1;
                    sh.Cells[headerRow + 1 + _currentPlayer++, currentColumn].Value2 = value;
                }

                if (columnSettings.IsBold)
                    columnRange.Font.Bold = true;

                sh.Columns[currentColumn].ColumnWidth = columnSettings.Width;

                if (columnSettings.Alignment == Alignment.Left)
                    columnRange.HorizontalAlignment = XlHAlign.xlHAlignLeft;                
            }
        }
    }
}
