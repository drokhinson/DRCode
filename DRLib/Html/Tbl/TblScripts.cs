using System.Drawing;
using System.Linq;
using DRLib.Html;
using DRLib.Html.Core;
using DRLib.Html.Css;
using DRLib.Html.Scripts;
using DRLib.Html.Styles;

// ReSharper disable once CheckNamespace
namespace AIMLib.HtmlWriter.Tbl.Scripts;

public record SortColumnScript : HtmlJScript
{
    public SortColumnScript() : base()
    {
        var posStyles = new[] {
            new HtmlStyle("position", "absolute"),
            new HtmlStyle("right", "10px"),
            new HtmlStyle("font-size", "12px"),
        };
        
        var asc = new HtmlClass("sorted-asc::after", [new HtmlStyle("content", "'▲'"), .. posStyles]) {
            Selector = ClassSelector.ClassName
        };

        var desc = new HtmlClass("sorted-desc::after", [new HtmlStyle("content", "'▼'"), .. posStyles]) {
            Selector = ClassSelector.ClassName
        };

        InitAttributes = [asc, desc];
    }

    protected override string GetScriptText()
    {
        return """
   
   let colSortDict = new Map();
   
   document.addEventListener('click', (event) => {

        table = event.target.closest('table');
        
        if (table != null && event.target.tagName === 'TH') {
            headerCell = event.target;
            colIndex = Array.from(headerCell.parentElement.children).indexOf(headerCell);
            sortTable(colIndex, table);
        }
   });
   
   function sortTable(columnIndex, table) {
       rows = Array.from(table.rows).slice(1); // Exclude header row
       isNumericColumn = columnIndex === 0 || columnIndex === 2; // ID and Age are numeric
   
        saved = colSortDict.get(table.Id);
        currentSortColumn = -1;
        currentSortDirection = 'asc';
        if(saved != null) {
            currentSortColumn = saved[0]
            currentSortDirection = saved[1]
        }

        if (currentSortColumn === columnIndex) {
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortDirection = 'asc';
            currentSortColumn = columnIndex;
        }

        colSortDict.set(table.Id, [currentSortColumn, currentSortDirection]);
       rows.sort((rowA, rowB) => {
           const cellA = rowA.cells[columnIndex].textContent.trim();
           const cellB = rowB.cells[columnIndex].textContent.trim();
   
           if (isNumericColumn) {
               return currentSortDirection === 'asc' ? parseInt(cellA) - parseInt(cellB) : parseInt(cellB) - parseInt(cellA);
           } else {
               return currentSortDirection === 'asc' ? cellA.localeCompare(cellB) : cellB.localeCompare(cellA);
           }
       });
   
       rows.forEach(row => table.tBodies[0].appendChild(row)); // Re-append sorted rows
   
       id = table.id;
       const headers = table.querySelectorAll('th');
       headers.forEach((header, index) => {
           computedStyle = window.getComputedStyle(header);
           
           if (computedStyle.position === 'static') 
                header.style.position = 'relative';

           header.classList.remove('sorted-asc', 'sorted-desc');
           if (index === currentSortColumn) {
               header.classList.add(currentSortDirection === 'asc' ? 'sorted-asc' : 'sorted-desc');
           }
       });
   }
   """;

    }
}

public record CellSelectScript : HtmlJScript
    {
        private HtmlClass _SelectStyle;
        private string SelectClassName => _SelectStyle.ClassName;
        public HtmlClass SelectCellStyle
        {
            get => _SelectStyle;
            set => OverrideStyle(value);
        }

        public CellSelectScript()
        {
            SelectCellStyle = new HtmlClass("JS_selected-cell", new BackColor(Color.FromArgb(176, 224, 230)));
        }

        private void OverrideStyle(HtmlClass newStyle)
        {
            RemoveAllAttributes(r => r is HtmlClass c && c.ClassName == SelectClassName);

            // Append important to ensure selection styles override other styles
            var withImportant = new HtmlClass(newStyle.ClassName) {
                Selector = ClassSelector.ClassName,
                Styles = newStyle.Styles.Select(r => new HtmlStyle(r.Property, $"{r.Value} !important")).ToList()
            };
            withImportant.Styles.Add(new HtmlStyle("user-select", "none"));

            _SelectStyle = withImportant;
            AddAttribute(withImportant);
        }

        protected override string GetScriptText()
        {
            return $$"""
       let selectedCells = [];
       let lastSelectedCell = null; // last selection
       let mainSelectedCell = null; // first cell selected in current range
       let isMouseDown = false;
       let ctrlDwn = false;
       
       document.addEventListener('click', (event) => {
            if(!mainSelectedCell)
               	return;
            
            table = event.target.closest('table');
            activeTable = mainSelectedCell.closest('table');
        
            if (!table || table != activeTable) {
                deselectAllCells();
            }
       });
       
       document.addEventListener('mousedown', (event) => {
           if (event.target.tagName === 'TD' && event.target.parentElement.parentElement.tagName === 'TBODY') 
           {
                isMouseDown = true;
                if(event.ctrlKey)
               	    selectCell(event.target, true);
                else if (event.shiftKey && mainSelectedCell){
               	    selectRange(mainSelectedCell, event.target)
                }
                else {
               	    deselectAllCells();
               	    selectCell(event.target, false);
                }
           }

       });
       
       document.addEventListener('mouseup', (event) => {
           isMouseDown = false;
       });
       
       document.addEventListener('keyup', (event) => {
           ctrlDwn = false;
       });
       
       document.addEventListener('mouseover', (event) => {
            if(!ctrlDwn == false)
               	return;
               	
           if (isMouseDown && event.target == mainSelectedCell)
               selectCell(event.target, false);
           else if (isMouseDown && event.target.tagName === 'TD') {
               selectRange(mainSelectedCell, event.target)
           }
       });
       
       document.addEventListener('keydown', (event) => {
           switch (event.key) {
               case 'ArrowUp':
               case 'ArrowDown':
               case 'ArrowLeft':
               case 'ArrowRight':
                   handleArrowKeys(event);
                   break;
               case 'a':
                   if (event.ctrlKey) {
                        selectAllCells();
               			event.preventDefault();
                   }
                   break;
               case 'Escape':
                   deselectAllCells();
                   break;
               case 'c':
                   if (event.ctrlKey) {
                       ctrlDwn = true; 
                       copySelectedCells();
                   }
                   break;
           }
       });
       
       function selectCell(cell, addToSelection = false) 
       {
           if (!addToSelection) {
                deselectAllCells();
               	mainSelectedCell = cell;
           }

           if (!selectedCells.includes(cell)) {
               cell.classList.add('{{SelectClassName}}');
               selectedCells.push(cell);
               lastSelectedCell = cell;
           } else if(addToSelection) {
               	cell.classList.remove('{{SelectClassName}}');
                selectedCells.pop(cell);
           }
       }
       
       function selectRange(startCell, endCell) 
       {
            table = startCell.closest('table');
            if(!table)
               	return;
            _main = mainSelectedCell;
            deselectAllCells();
            mainSelectedCell = _main;
            
            const startRowIndex = startCell.parentElement.rowIndex - 1; // Adjust for tbody
           const startCellIndex = Array.from(startCell.parentElement.children).indexOf(startCell);
           const endRowIndex = endCell.parentElement.rowIndex - 1;
           const endCellIndex = Array.from(endCell.parentElement.children).indexOf(endCell);
       
           const minRow = Math.min(startRowIndex, endRowIndex);
           const maxRow = Math.max(startRowIndex, endRowIndex);
           const minCell = Math.min(startCellIndex, endCellIndex);
           const maxCell = Math.max(startCellIndex, endCellIndex);
            
           for (let i = minRow; i <= maxRow; i++) {
               for (let j = minCell; j <= maxCell; j++) {
                   const cell = table.tBodies[0].rows[i].cells[j];
                   selectCell(cell, true);
               }
           }
       }
       
       function deselectAllCells() 
       {
            selectedCells.forEach(cell => cell.classList.remove('{{SelectClassName}}'));
            selectedCells = [];
            lastSelectedCell = null;
            mainSelectedCell = null;
       }
       
       function selectAllCells() 
       {
            if(!lastSelectedCell)
               	return;
            
            table = lastSelectedCell.closest('table');
            bodyCells = table.getElementsByTagName('td');
            selectedCells = [];
            Array.from(bodyCells).forEach(cell => {
                cell.classList.add('{{SelectClassName}}');
                selectedCells.push(cell);
            });
        
            window.getSelection().empty();
       }

       function copySelectedCells() 
       {
            const rows = {};
       
           selectedCells.forEach(cell => {
               const rowIndex = cell.parentElement.rowIndex;
               if (!rows[rowIndex]) {
                   rows[rowIndex] = [];
               }
               rows[rowIndex].push(cell.textContent.trim());
           });
       
           const text = Object.values(rows)
               .map(row => row.join('\t'))
               .join('\n');
       
            navigator.clipboard.writeText(text)
       }
       
       function handleArrowKeys(event) 
       {
            if (!lastSelectedCell) 
                return;

            table = lastSelectedCell.closest('table');
            currentRow = lastSelectedCell.parentElement;
            currentRowIndex = Array.from(currentRow.parentElement.children).indexOf(currentRow);
            currentCellIndex = Array.from(currentRow.children).indexOf(lastSelectedCell);
            let newRowIndex = currentRowIndex;
            let newCellIndex = currentCellIndex;

            switch (event.key) {
                case 'ArrowUp':
               		if (event.ctrlKey) {
                        newRowIndex = 0;
                    } else {
               			newRowIndex = Math.max(0, currentRowIndex - 1);
               		}
                    break;
                case 'ArrowDown':
               		if (event.ctrlKey) {
                        newRowIndex = table.tBodies[0].rows.length - 1;
                    } else {
               			newRowIndex = Math.min(table.tBodies[0].rows.length - 1, currentRowIndex + 1);
               		}
                    break;
                case 'ArrowLeft':
               		if (event.ctrlKey) {
                        newCellIndex = 0;
                    } else {
               			newCellIndex  = Math.max(0, currentCellIndex - 1);
               		}
                    break;
                case 'ArrowRight':
               		if (event.ctrlKey) {
                        newCellIndex = currentRow.cells.length - 1;
                    } else {
               			newCellIndex  = Math.min(currentRow.cells.length - 1, currentCellIndex + 1);
               		}
                    break;
            }

            newCell = table.tBodies[0].rows[newRowIndex].cells[newCellIndex];

            if (event.shiftKey && event.ctrlKey) {
                selectRange(mainSelectedCell, newCell);
                lastSelectedCell = newCell;
            } else if (event.shiftKey) {
               	selectRange(mainSelectedCell, newCell);
               	lastSelectedCell = newCell;
            }
            else {
                selectCell(newCell, false);
            }
       }
       """;
        }
    }
