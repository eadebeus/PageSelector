using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

//
// PageSelector implements an iterator that enables the caller to iterate
// through selected pages in a PDF file. For example:
//   PageSelector ps = new PageSelector() {
//                             ParitySelection = ParitySelection.Even;
//                             BookmarkSelection = BookmarkSelection.MatchContains;
//                             BookmarkString = "media" 
//                             PageSelection = PageSelection.PageRanges,
//                             PageRanges = "1-10,30-46,50" 
//                      };
// will produce an iterator that will return the pages in a PDF file that are
// even, in the range "1-10,30-46,50", and have bookmarks that contain the
// text "media".
//
// For clarity, input validation, error handling, and logging have been largely omitted.
//


namespace PageSelectorDemo
{
    public enum PageSelection { All, PageRanges }

    public enum OrientationSelection { None, Portrait, Landscape }

    public enum ParitySelection { None, Even, Odd }

    public enum BookmarkSelection {
        DontMatch,
        MatchAny,
        MatchEquals,
        MatchContains,
        MatchNotEqual,
        MatchDoesNotContain
    }

    public class PageSelector : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            return new PageIterator(this);
        }

        public IEnumerable<int> GetPages()
        {
            IEnumerator iter = GetEnumerator();
            while(iter.MoveNext()) 
                yield return (int) iter.Current;
        }

        public PageSelection PageSelection { get; set; }     
        public OrientationSelection OrientationSelection { get; set; }
        public ParitySelection ParitySelection { get; set; }
        public BookmarkSelection BookmarkSelection { get; set; }
        public string BookmarkString { get; set; }   
        public List<PageRange> PageRanges { get; set; }
        private int NumPages { get; set; }
        private OrientationSelection[] PageOrientationTable { get; set; }
        private List<string>[] PageBookmarkTable { get; set; }

        public bool IsEven() {
            return ParitySelection == ParitySelection.Even;
        }

        public bool IsOdd() {
            return ParitySelection == ParitySelection.Odd;
        }

        public int NumRanges {
            get { return PageSelection == PageSelection.PageRanges && PageRanges != null ? PageRanges.Count : 1; }
        }

        public OrientationSelection GetPageOrientation(int iPage) {
            if(PageOrientationTable == null || iPage < 1 || iPage > PageOrientationTable.Length)
                return OrientationSelection.None;
            return PageOrientationTable[iPage - 1];
        }
        
        public List<string> GetPageBookmarks(int iPage) {
            if(PageBookmarkTable == null || iPage < 1 || iPage > PageBookmarkTable.Length)
                return null;
            return PageBookmarkTable[iPage - 1];
        }

        public PageSelector() {
            NumPages = 0;
            PageRanges = null;
            PageSelection = PageSelection.All;
            OrientationSelection = OrientationSelection.None;
            ParitySelection = ParitySelection.None;
            BookmarkSelection = BookmarkSelection.DontMatch;
            BookmarkString = null;
        }

        public void ReadFileInformation(ISourceFile sourceFile) {
            try {
                NumPages = sourceFile.NumPages;
                if(NumPages <= 0)
                    return;
                PageOrientationTable = new OrientationSelection[NumPages];
                PageBookmarkTable = new List<string>[NumPages];
                for(int iPage = 1; iPage <= NumPages; iPage++) {
                    PageSize pageSize = sourceFile.GetPageSize(iPage);
                    PageOrientationTable[iPage - 1] = pageSize.Width > pageSize.Height ? OrientationSelection.Landscape : OrientationSelection.Portrait;
                    PageBookmarkTable[iPage - 1] = sourceFile.GetBookmarksForPage(iPage);
                }
            }
            catch(Exception) {
            }
        }

        public void SetPageRanges(string pageRanges)
        {
            PageRanges = ParsePageRanges(pageRanges).ToList();
            PageSelection = PageSelection.PageRanges;
        }

        private IEnumerable<PageRange> ParsePageRanges(string pageRanges)
        {
            foreach(string s in pageRanges.Split(',')) {
                int iStart, iEnd;
                if(int.TryParse(s, out iStart)) {
                    yield return new PageRange() { Start = iStart, End = iStart };
                    continue;
                }

                string[] range = s.Split('-');
                if(range.Length >= 2 && 
                    int.TryParse(range[0], out iStart) && 
                    int.TryParse(range[1], out iEnd) &&
                    iStart <= iEnd) {
                        yield return new PageRange() { Start = iStart, End = iEnd };
                }
            }
        }

        private class PageIterator : IEnumerator<int>
        {
            private PageSelector Selector { get; set; }
            private int CurrentPage { get; set; }
            private int CurrentMaxPage { get; set; }
            private int CurrentIncrement { get; set; }
            private int CurrentRange { get; set; }

            public PageIterator(PageSelector selector)
            {
                Selector = selector;
                Reset();
            }

            public bool MoveNext()
            {
                do {
                    if(CurrentPage == 0 || CurrentPage >= CurrentMaxPage) {
                        if(!UpdateRange())
                            return false;
                    }
                    else
                        CurrentPage += CurrentIncrement;

                    if(CurrentPage > CurrentMaxPage) {
                        if(!UpdateRange())
                            return false;
                    }

                    if(IsParityMismatch() || IsOrientationMismatch() || IsBookmarkMismatch())
                        continue;

                    break;

                } while(true);

                return true;
            }

            public void Reset()
            {
                CurrentRange = 0;
                CurrentPage = 0;
                CurrentMaxPage = 0;
                CurrentIncrement = 1;
            }

            public int Current {
                get { return CurrentPage; }
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            void IDisposable.Dispose() { }

            private bool IsParityMismatch()
            {
                return ((Selector.IsOdd() && (CurrentPage & 0x01) == 0) || (Selector.IsEven() && (CurrentPage & 0x01) != 0));
            }

            private bool IsOrientationMismatch()
            {
                if(Selector.OrientationSelection == OrientationSelection.None)
                    return false;
                return (Selector.OrientationSelection != Selector.GetPageOrientation(CurrentPage));
            }

            private bool IsBookmarkMismatch()
            {
                return !IsBookmarkMatch();
            }

            private bool IsBookmarkMatch()
            {
                List<string> bookmarks = Selector.BookmarkSelection != BookmarkSelection.DontMatch ? Selector.GetPageBookmarks(CurrentPage) : null;
                string regex = null;
                bool bEmpty = string.IsNullOrEmpty(Selector.BookmarkString);
                if(Selector.BookmarkString == null)
                    Selector.BookmarkString = "";
                switch(Selector.BookmarkSelection) {
                    case BookmarkSelection.DontMatch:
                        return true;
                    case BookmarkSelection.MatchAny:
                        return bookmarks != null;
                    case BookmarkSelection.MatchEquals:
                    case BookmarkSelection.MatchContains:
                        if(bEmpty || bookmarks == null)
                            return false;
                        regex = CreateRegex(Selector.BookmarkSelection, Selector.BookmarkString);
                        return IsRegexMatch(regex, bookmarks);
                    case BookmarkSelection.MatchNotEqual:
                    case BookmarkSelection.MatchDoesNotContain:
                        if(bookmarks == null)
                            return bEmpty;
                        regex = CreateRegex(Selector.BookmarkSelection, Selector.BookmarkString);
                        return IsRegexMatch(regex, bookmarks);
                    default:
                        return false;
                }
            }

            private bool IsRegexMatch(string expr, List<string> bookmarks)
            {
                if(string.IsNullOrEmpty(expr))
                    return false;
                Regex rx = new Regex(expr);
                foreach(string bookmark in bookmarks) {
                    if(rx.IsMatch(bookmark))
                        return true;
                }
                return false;
            }

            private string CreateRegex(BookmarkSelection matchType, string matchText)
            {
                string regex = null;

                if(string.IsNullOrEmpty(matchText))
                    return null;

                string raw = null;
                foreach(char c in matchText)
                    raw += char.IsLetterOrDigit(c) ? c.ToString() : Regex.Escape(c.ToString());
                switch(matchType) {
                    case BookmarkSelection.MatchContains:
                        regex = raw;
                        break;
                    case BookmarkSelection.MatchDoesNotContain:
                        regex = "^((?!" + raw + ").)*$";
                        break;
                    case BookmarkSelection.MatchEquals:
                        regex = "^" + raw + "$";
                        break;
                    case BookmarkSelection.MatchNotEqual:
                        regex = "^(?!" + raw + "$)";
                        break;
                    default:
                        break;
                }

                return regex;
            }

            private bool UpdateRange()
            {
                int iNumPages = Selector.NumPages;

                while(true) {
                    if(Selector.PageSelection == PageSelection.All) {
                        if(CurrentPage == 0) {
                            CurrentPage = 1;
                            CurrentMaxPage = iNumPages;
                            CurrentIncrement = 1;
                        }
                        else if(CurrentPage >= iNumPages)
                            return false;
                    }
                    else if(Selector.PageSelection == PageSelection.PageRanges) {
                        if(Selector.PageRanges == null)
                            return false;
                        if(CurrentRange >= Selector.NumRanges)
                            return false;
                        CurrentPage = Selector.PageRanges[CurrentRange].Start;
                        CurrentMaxPage = Selector.PageRanges[CurrentRange].End;
                        CurrentRange++;

                        CurrentIncrement = 1;
                        CurrentPage = NormalizePage(CurrentPage);
                        if(CurrentPage > iNumPages)
                            continue;
                        CurrentMaxPage = NormalizePage(CurrentMaxPage);
                        if(CurrentPage > CurrentMaxPage)
                            continue;
                    }

                    break;
                }

                return true;
            }

            private int NormalizePage(int iPage)
            {
                return iPage <= 0 ? 1 : (iPage > Selector.NumPages ? Selector.NumPages : iPage);
            }
        }
    }

    public class PageRange
    {
        public int Start;
        public int End;
    }

}
